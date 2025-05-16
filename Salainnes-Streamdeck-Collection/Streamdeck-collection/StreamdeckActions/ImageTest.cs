using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;
using Streamdeck_collection.Model;
using Streamdeck_collection.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Streamdeck_collection.StreamdeckActions
{
    [StreamdeckAction("com.salainne.salainnes-streamdeck-collection.ImageTest")]
    public static class ImageTest
    {
        public static void Run(StreamDeckConnection connection, KeyValuePair<string, ActiveIconContainer> icon)
        {
            if(icon.Value.Data == null)
            {
                icon.Value.Data = new ImageTestData() { UpdateInterval = 10000 };
            }
            var data = icon.Value.Data as ImageTestData;
            if (data.Image == null || data.LastUpdate.AddMilliseconds(data.UpdateInterval) < DateTime.Now)
            {
                data.Image = GetRandomImageAsBase64();
                data.LastUpdate = DateTime.Now;
            }
            var tmp = DateTime.Now.ToString("HH:mm:ss");
            _ = connection.SetImageAsync(data.Image, icon.Key, SDKTarget.HardwareAndSoftware, null);
            //_ = connection.SetImageAsync(_img, icon.Key, SDKTarget.HardwareAndSoftware, null);
            _ = connection.SetTitleAsync(tmp, icon.Key, SDKTarget.HardwareAndSoftware, null);

        }

        private static string ImageToBase64(System.Drawing.Image img)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                img.Save(memoryStream, ImageFormat.Png);
                byte[] inArray = memoryStream.ToArray();
                string base64Image = "data:image/png;base64," + Convert.ToBase64String(inArray);
                return base64Image;
            }
        }

        public static System.Drawing.Image DownloadRandomImage()
        {
            using (var client = new WebClient())
            {
                // Hent bytes fra URL
                byte[] data = client.DownloadData("http://picsum.photos/72");

                // Lav det om til en MemoryStream
                using (var stream = new System.IO.MemoryStream(data))
                {
                    // Skab Image fra stream
                    return System.Drawing.Image.FromStream(stream);
                }
            }
        }

        public static string GetRandomImageAsBase64()
        {
            using (var client = new WebClient())
            {
                // Hent bytes fra URL
                byte[] data = client.DownloadData("http://picsum.photos/72");

                var base64Image = "data:image/png;base64," + Convert.ToBase64String(data);
                return base64Image;
            }
        }
    }
}
