using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Streamdeck_collection.Helpers
{
    public static class SystemMonitor
    {
        private static readonly Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true
        };

        private static readonly List<PerformanceCounterWithValue> cpuCoreCounters = new List<PerformanceCounterWithValue>();
        private static readonly PerformanceCounterWithValue cpuTotal = new PerformanceCounterWithValue(new PerformanceCounter("Processor", "% Processor Time", "_Total"));
        private static readonly PerformanceCounterWithValue baseFrequencyMHzCounter = new PerformanceCounterWithValue(new PerformanceCounter("Processor Information", "Processor Frequency", "_Total", true));
        private static readonly PerformanceCounterWithValue perfStateCounter = new PerformanceCounterWithValue(new PerformanceCounter("Processor Information", "% Processor Performance", "_Total", true));
        private static readonly List<GpuData> gpuDataToCollect = new List<GpuData>();

        public static uint MemoryLoad { get; private set; }

        private class PerformanceCounterWithValue
        {
            private readonly PerformanceCounter _counter;
            public float Value
            {
                get; private set;
            }

            public PerformanceCounterWithValue(PerformanceCounter counter)
            {
                _counter = counter;
            }

            public void Update()
            {
                Value = _counter.NextValue();
            }
        }
        private class GpuData
        {
            public SensorType Sensor { get; private set; }
            public string Name { get; private set; }
            public string NotInName { get; private set; }
            public float Value { get; private set; }

            public GpuData(SensorType sensor, string name, string notInName = "")
            {
                Sensor = sensor;
                Name = name;
                NotInName = notInName;
            }

            public void SetValue(float value)
            {
                Value = value;
            }
        }


        static SystemMonitor()
        {
            computer.Open();

            int coreCount = Environment.ProcessorCount;
            for (int i = 0; i < coreCount; i++)
            {
                cpuCoreCounters.Add(new PerformanceCounterWithValue(new PerformanceCounter("Processor", "% Processor Time", i.ToString(), true)));
            }
            gpuDataToCollect.Add(new GpuData(SensorType.Load, "Core"));
            gpuDataToCollect.Add(new GpuData(SensorType.Temperature, "Hot Spot"));
            gpuDataToCollect.Add(new GpuData(SensorType.Temperature, "Core"));
            gpuDataToCollect.Add(new GpuData(SensorType.SmallData, "Memory Used"));
            gpuDataToCollect.Add(new GpuData(SensorType.Load, "GPU Memory", "Controller"));
            UpdateAllCounters();
        }

        public static void UpdateAllCounters()
        {
            UpdatePerformanceCounters();
            UpdateMemoryCounters();
            UpdateGpuCounters();
        }
        public static void UpdatePerformanceCounters()
        {
            foreach (var counter in cpuCoreCounters)
            {
                counter.Update();
            }
            baseFrequencyMHzCounter.Update();
            perfStateCounter.Update();
            cpuTotal.Update();
        }
        public static void UpdateMemoryCounters()
        {
            MemoryLoad = GetMemoryLoad();
        }
        public static void UpdateGpuCounters()
        {
            foreach (var hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    {
                        foreach (var gpuData in gpuDataToCollect)
                        {
                            if (sensor.SensorType == gpuData.Sensor && sensor.Name.Contains(gpuData.Name))
                            {
                                if (!string.IsNullOrEmpty(gpuData.NotInName) && sensor.Name.Contains(gpuData.NotInName))
                                    continue;
                                gpuData.SetValue(sensor.Value.GetValueOrDefault());
                            }
                        }
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MEMORYSTATUSEX
        {
            public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        private static uint GetMemoryLoad()
        {
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                return memStatus.dwMemoryLoad;
            }
            else
            {
                throw new InvalidOperationException("GlobalMemoryStatusEx fejlede.");
            }
        }

        public static (ulong used, ulong total) GetMemoryInfo()
        {
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                ulong used = memStatus.ullTotalPhys - memStatus.ullAvailPhys;
                ulong total = memStatus.ullTotalPhys;
                return (used, total);
            }
            else
            {
                throw new InvalidOperationException("GlobalMemoryStatusEx fejlede.");
            }
        }

        public static float GetCpuLoadCorrectedForActiveCores()
        {
            float totalLoad = 0;
            int activeCores = 0;

            foreach (var counter in cpuCoreCounters)
            {
                if (counter.Value > 5.0f)
                {
                    totalLoad += counter.Value;
                    activeCores++;
                }
            }

            if (activeCores == 0) activeCores = 1;
            return totalLoad / activeCores;
        }

        public static float GetCpuLoad()
        {
            return cpuTotal.Value;
        }

        public static float GetNominalCpuFrequency()
        {
            return baseFrequencyMHzCounter.Value;
        }

        public static float GetCurrentCpuFrequency()
        {
            float performancePercent = perfStateCounter.Value;
            return baseFrequencyMHzCounter.Value * (performancePercent / 100f);
        }

        public static float GetCpuTemperature()
        {
            foreach (var hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    hardware.Update();

                    // Først prøv at finde "CPU Package"
                    var packageSensor = hardware.Sensors.FirstOrDefault(s =>
                        s.SensorType == SensorType.Temperature && s.Name.Contains("Package"));
                    if (packageSensor != null)
                        return packageSensor.Value.GetValueOrDefault();

                    // Hvis ikke, prøv "Core Max"
                    var coreMaxSensor = hardware.Sensors.FirstOrDefault(s =>
                        s.SensorType == SensorType.Temperature && s.Name.Contains("Core Max"));
                    if (coreMaxSensor != null)
                        return coreMaxSensor.Value.GetValueOrDefault();

                    // Ellers brug "Core Average"
                    var coreAvgSensor = hardware.Sensors.FirstOrDefault(s =>
                        s.SensorType == SensorType.Temperature && s.Name.Contains("Core Average"));
                    if (coreAvgSensor != null)
                        return coreAvgSensor.Value.GetValueOrDefault();

                    // Til sidst gennemsnit af alle kerner hvis alt andet fejler
                    var coreTemps = hardware.Sensors
                        .Where(s => s.SensorType == SensorType.Temperature && s.Name.StartsWith("CPU Core #"))
                        .Select(s => s.Value.GetValueOrDefault())
                        .ToList();

                    if (coreTemps.Count > 0)
                        return coreTemps.Average();
                }
            }

            return 0;
        }



        public static float GetGpuLoad()
        {
            return gpuDataToCollect.FirstOrDefault(x => x.Sensor == SensorType.Load && x.Name == "Core").Value;
            //return GetGpuData(SensorType.Load, "Core");
        }
        public static float GetGpuHotSpotTemperature()
        {
            return gpuDataToCollect.FirstOrDefault(x => x.Sensor == SensorType.Temperature && x.Name == "Hot Spot").Value;
            //return GetGpuData(SensorType.Temperature, "Hot Spot");
        }
        public static float GetGpuTemperature()
        {
            return gpuDataToCollect.FirstOrDefault(x => x.Sensor == SensorType.Temperature && x.Name == "Core").Value;
            //return GetGpuData(SensorType.Temperature, "Core");
        }
        public static float GetVramUsage()
        {
            return gpuDataToCollect.FirstOrDefault(x => x.Sensor == SensorType.SmallData && x.Name == "Memory Used").Value;
            return GetGpuData(SensorType.SmallData, "Memory Used");
        }
        public static float GetVramUsagePercent()
        {
            return gpuDataToCollect.FirstOrDefault(x => x.Sensor == SensorType.Load && x.Name == "GPU Memory").Value;
            //return GetGpuData(SensorType.Load, "GPU Memory", "Controller");
        }

        private static float GetGpuData(SensorType sensorType, string name, string notInName = "")
        {
            foreach (var hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == sensorType && sensor.Name.Contains(name))
                        {
                            if(!string.IsNullOrEmpty(notInName) && sensor.Name.Contains(notInName))
                                continue;

                            return sensor.Value.GetValueOrDefault();
                        }
                    }
                }
            }
            return 0;
        }

        public static PowerPlan GetActivePowerPlan()
        {
            var process = new Process();
            process.StartInfo.FileName = "powercfg";
            process.StartInfo.Arguments = "/query";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            var tmp = lines[0].Substring(lines[0].IndexOf("GUID") + 5).Trim();
            var name = tmp.Substring(tmp.IndexOf(" ")).Trim();
            var guid = tmp.Replace(name, "");


            var plan = new PowerPlan
            {
                Name = name.Replace("(", "").Replace(")", "").Trim(),
                Guid = Guid.Parse(guid.Trim())
            };

            return plan;
        }
    }

    //--
    public class PowerPlan
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
    }
}
