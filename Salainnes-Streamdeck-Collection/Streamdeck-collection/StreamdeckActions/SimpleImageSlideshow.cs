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
            }
            var data = icon.Value.Data as SimpleImageSlideshowData;

            if (icon.Value.HasNewSettings)
            {
                data.Parse(icon.Value.RawSettings);
                icon.Value.HasNewSettings = false;
                data.LastUpdate = DateTime.MinValue; // force image update
            }

            //var tmp = DateTime.Now.ToString("HH:mm:ss");
            if (data.Image == null || data.LastUpdate.AddMilliseconds(data.UpdateInterval) < DateTime.Now)
            {
                data.Image = GetRandomImageAsBase64(data);
                data.LastUpdate = DateTime.Now;
                _ = connection.SetImageAsync(data.Image, icon.Key, SDKTarget.HardwareAndSoftware, null);
            }

            var test = connection.GetSettingsAsync(icon.Key);

            //var tmp = "hej";
            //_ = connection.SetTitleAsync(tmp, icon.Key, SDKTarget.HardwareAndSoftware, null);
        }

        private static string GetRandomImageAsBase64(SimpleImageSlideshowData settings)
        {
            try
            {
                using (var client = new WebClient())
                {
                    byte[] data = client.DownloadData(settings.Url);
                    var base64Image = "data:image/png;base64," + Convert.ToBase64String(data);
                    return base64Image;
                }
            }
            catch
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    Resources.error.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] errorData = ms.ToArray();
                    var base64Image = "data:image/png;base64," + Convert.ToBase64String(errorData);
                    return base64Image;
                }
            }
        }
    }
}
