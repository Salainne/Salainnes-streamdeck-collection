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
    [StreamdeckAction("com.salainne.salainnes-streamdeck-collection.CPULoad")]
    public static class CpuLoad
    {
        public static void Run(StreamDeckConnection connection, KeyValuePair<string, ActiveIconContainer> icon)
        {
            //var load = Helpers.SystemMonitor.GetCpuLoadCorrectedForActiveCores();
            var load = Helpers.SystemMonitor.GetCpuLoad();

            //var tmp = $"CPU{Environment.NewLine}{load:####.##}%";
            var tmp = $"CPU{Environment.NewLine}{load:####}%";
            _ = connection.SetTitleAsync(tmp, icon.Key, SDKTarget.HardwareAndSoftware, null);
        }
    }
}
