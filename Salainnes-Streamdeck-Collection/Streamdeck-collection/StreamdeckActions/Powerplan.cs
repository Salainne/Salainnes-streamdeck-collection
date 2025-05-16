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
    [StreamdeckAction("com.salainne.salainnes-streamdeck-collection.Powerplan")]
    public static class Powerplan
    {
        public static void Run(StreamDeckConnection connection, KeyValuePair<string, ActiveIconContainer> icon)
        {
            var powerPlan = Helpers.SystemMonitor.GetActivePowerPlan();

            var name = powerPlan.Name.Replace(" ", Environment.NewLine);

            var tmp = $"Powerplan{Environment.NewLine}{name}";
            _ = connection.SetTitleAsync(tmp, icon.Key, SDKTarget.HardwareAndSoftware, null);
        }
    }
}
