using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;
using Streamdeck_collection.Model;
using Streamdeck_collection.Model.Attributes;
using Streamdeck_collection.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Streamdeck_collection.StreamdeckActions
{
    [StreamdeckAction("com.salainne.salainnes-streamdeck-collection.SimpleImageSlideshow")]
    public static class SimpleImageSlideshow
    {
        public static void Run(StreamDeckConnection connection, KeyValuePair<string, ActiveIconContainer> icon)
        {
            if (icon.Value.Data == null)
            {
                icon.Value.Data = new SimpleImageSlideshowData();
                _ = connection.SetImageAsync("", icon.Key, SDKTarget.HardwareAndSoftware, null);
                _ = connection.SetTitleAsync("", icon.Key, SDKTarget.HardwareAndSoftware, null);
            }
            var data = icon.Value.Data as SimpleImageSlideshowData;

            if (icon.Value.HasNewSettings || icon.Value.HasHadKeyDown)
            {
                data.Parse(icon.Value.RawSettings);
                icon.Value.HasNewSettings = false;
                data.LastUpdate = DateTime.MinValue; // force image update
            }

            //var tmp = DateTime.Now.ToString("HH:mm:ss");
            if (data.Image == null || data.LastUpdate.AddMilliseconds(data.UpdateInterval) < DateTime.Now)
            {
                data.Image = Helpers.Utilities.GetRandomImageAsBase64(data);
                data.LastUpdate = DateTime.Now;
                _ = connection.SetImageAsync(data.Image, icon.Key, SDKTarget.HardwareAndSoftware, null);
            }

            //var test = connection.GetSettingsAsync(icon.Key);

            //var tmp = "hej";
            //_ = connection.SetTitleAsync(tmp, icon.Key, SDKTarget.HardwareAndSoftware, null);

            // reset keydown/up
            icon.Value.HasHadKeyDown = false;
            icon.Value.HasHadKeyUp = false;
            
        }
    }
}
