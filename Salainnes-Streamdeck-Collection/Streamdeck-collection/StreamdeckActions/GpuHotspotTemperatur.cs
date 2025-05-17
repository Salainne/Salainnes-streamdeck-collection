using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;
using Streamdeck_collection.Model;
using Streamdeck_collection.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamdeck_collection.StreamdeckActions
{
    [StreamdeckAction("com.salainne.salainnes-streamdeck-collection.GpuHotspotTemperatur")]
    public static class GpuHotspotTemperatur
    {
        public static void Run(StreamDeckConnection connection, KeyValuePair<string, ActiveIconContainer> icon)
        {
            if (icon.Value.Data == null)
            {
                icon.Value.Data = new object();
                _ = connection.SetImageAsync("", icon.Key, SDKTarget.HardwareAndSoftware, null);
            }
            var temp = Helpers.SystemMonitor.GetGpuHotSpotTemperature();

            var tmp = $"Hotspot{Environment.NewLine}{temp:####}°C";
            _ = connection.SetTitleAsync(tmp, icon.Key, SDKTarget.HardwareAndSoftware, null);
        }
    }
}
