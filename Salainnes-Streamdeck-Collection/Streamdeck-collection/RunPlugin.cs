using Newtonsoft.Json.Linq;
using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;
using Streamdeck_collection.Helpers;
using Streamdeck_collection.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Streamdeck_collection
{
    internal partial class Program
    {
        private static Dictionary<string, ActiveIconContainer> _ActiveIcons = new Dictionary<string, ActiveIconContainer>();

        static void RunPlugin(StreamdeckOptions options)
        {
            ManualResetEvent connectEvent = new ManualResetEvent(false);
            ManualResetEvent disconnectEvent = new ManualResetEvent(false);

            StreamDeckConnection connection = new StreamDeckConnection(options.Port, options.PluginUUID, options.RegisterEvent);

            RegisterStandardEventHandlers(connection, connectEvent, disconnectEvent);

            // Start the connection
            connection.Run();

            //-- main loop
            // Wait for up to 10 seconds to connect
            if (connectEvent.WaitOne(TimeSpan.FromSeconds(10)))
            {
                // We connected, loop every second until we disconnect
                while (!disconnectEvent.WaitOne(TimeSpan.FromMilliseconds(1000)))
                {
                    SystemMonitor.UpdateAllCounters();

                    lock (_ActiveIcons)
                    {
                        foreach (var icon in _ActiveIcons)
                        {
                            try
                            {
                                if (_actionHandlers.TryGetValue(icon.Value.WillAppearEvent.Action, out var method))
                                {
                                    method.Invoke(null, new object[] { connection, icon });
                                }
                                else
                                {
                                    _ = connection.SetTitleAsync("MISSING", icon.Key, SDKTarget.HardwareAndSoftware, null);
                                }
                            }
                            catch(Exception ex){
                                _ = connection.SetTitleAsync(ex.Message, icon.Key, SDKTarget.HardwareAndSoftware, null);
                            }
                            
                        }
                    }


                    //lock (_Counters)
                    //{
                    //    foreach (KeyValuePair<string, string> kvp in _Counters.ToArray())
                    //    {
                    //        var cnt = -1;
                    //        Process[] processes = Process.GetProcesses();
                    //        var invoke = processes.Where(a => a.MainWindowTitle.Contains("Invoke - Community Edition")).ToList();
                    //        foreach (var proc in invoke)
                    //        {
                    //            cnt = 0;
                    //            if (proc.MainWindowTitle.Contains("("))
                    //            {
                    //                var startIndex = proc.MainWindowTitle.IndexOf("(");
                    //                var endIndex = proc.MainWindowTitle.IndexOf(")");
                    //                var sNum = proc.MainWindowTitle.Substring(startIndex + 1, endIndex - 1);

                    //                int.TryParse(sNum, out cnt);
                    //            }
                    //            //Console.WriteLine(cnt);
                    //        }
                    //        if (cnt < 0)
                    //        {
                    //            _Counters[kvp.Key] = _invokeConst + "n/a";
                    //        }
                    //        else
                    //        {
                    //            _Counters[kvp.Key] = _invokeConst + cnt.ToString();
                    //        }

                    //        _ = connection.SetTitleAsync(kvp.Value, kvp.Key, SDKTarget.HardwareAndSoftware, null);
                    //    }
                    //}
                }
            }
        }

        private static void RegisterStandardEventHandlers(StreamDeckConnection connection, ManualResetEvent connectEvent, ManualResetEvent disconnectEvent)
        {
            connection.OnConnected += (sender, args) =>
            {
                connectEvent.Set();
            };

            connection.OnDisconnected += (sender, args) =>
            {
                disconnectEvent.Set();
            };

            connection.OnApplicationDidLaunch += (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"App Launch: {args.Event.Payload.Application}");
            };

            connection.OnApplicationDidTerminate += (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"App Terminate: {args.Event.Payload.Application}");
            };

            //-- initialize data
            connection.OnWillAppear += (sender, args) =>
            {
                lock (_ActiveIcons)
                {
                    if (_ActiveIcons.ContainsKey(args.Event.Context) == false)
                    {
                        _ActiveIcons.Add(args.Event.Context, new ActiveIconContainer { WillAppearEvent = args.Event, RawSettings = args.Event.Payload.Settings });
                        Debug.WriteLine($"Icon loaded: {args.Event.Context}");
                    }
                }
            };

            connection.OnDidReceiveSettings += (sender, args) =>
            {
                lock (_ActiveIcons)
                {
                    if (_ActiveIcons.ContainsKey(args.Event.Context))
                    {
                        if(_ActiveIcons[args.Event.Context].RawSettings.ToString().GetHashCode() == args.Event.Payload.Settings.ToString().GetHashCode())
                        {
                            return;
                        }
                        _ActiveIcons[args.Event.Context].RawSettings = args.Event.Payload.Settings;
                        Debug.WriteLine($"Updating settings: {args.Event.Context}");
                    }
                }
            };

            connection.OnWillDisappear += (sender, args) =>
            {
                lock (_ActiveIcons)
                {
                    if (_ActiveIcons.ContainsKey(args.Event.Context))
                    {
                        _ActiveIcons.Remove(args.Event.Context);
                        Debug.WriteLine($"Icon unloaded: {args.Event.Context}");
                    }
                }
            };

            connection.OnKeyDown += (sender, args) =>
            {
                lock (_ActiveIcons)
                {
                    if (_ActiveIcons.ContainsKey(args.Event.Context))
                    {
                        _ActiveIcons[args.Event.Context].HasHadKeyDown = true;
                        Debug.WriteLine($"Key Down: {args.Event.Context}");
                    }
                }
            };
            connection.OnKeyUp += (sender, args) =>
            {
                lock (_ActiveIcons)
                {
                    if (_ActiveIcons.ContainsKey(args.Event.Context))
                    {
                        _ActiveIcons[args.Event.Context].HasHadKeyUp = true;
                        Debug.WriteLine($"Key Up: {args.Event.Context}");
                    }
                }
            };
            //--
        }
    }
}
