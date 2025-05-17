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
    [StreamdeckAction("com.salainne.salainnes-streamdeck-collection.MemoryLoad")]
    public static class MemoryLoad
    {
        public static void Run(StreamDeckConnection connection, KeyValuePair<string, ActiveIconContainer> icon)
        {
            if (icon.Value.Data == null)
            {
                icon.Value.Data = new object();
                _ = connection.SetImageAsync("", icon.Key, SDKTarget.HardwareAndSoftware, null);
            }
            var load = Helpers.SystemMonitor.MemoryLoad;

            var tmp = $"RAM{Environment.NewLine}{load}%";
            _ = connection.SetTitleAsync(tmp, icon.Key, SDKTarget.HardwareAndSoftware, null);
        }
    }
}
