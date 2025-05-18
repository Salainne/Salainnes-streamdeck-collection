using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;
using Streamdeck_collection.Model;
using Streamdeck_collection.Model.Attributes;
using Streamdeck_collection.Properties;
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
    [StreamdeckAction("com.salainne.salainnes-streamdeck-collection.ImageTransparencyTest")]
    public static class ImageTransparencyTest
    {
        public static void Run(StreamDeckConnection connection, KeyValuePair<string, ActiveIconContainer> icon)
        {
            if (icon.Value.Data == null)
            {
                icon.Value.Data = new ImageTestData() { UpdateInterval = 10000 };
                _ = connection.SetImageAsync("", icon.Key, SDKTarget.HardwareAndSoftware, null);
                _ = connection.SetTitleAsync("", icon.Key, SDKTarget.HardwareAndSoftware, null);
            }
            

            var tmp = DateTime.Now.ToString("HH:mm:ss");
            _ = connection.SetImageAsync(Helpers.Utilities.GetBase64Image(Resources.transparency), icon.Key, SDKTarget.HardwareAndSoftware, null);
            //_ = connection.SetImageAsync(_img, icon.Key, SDKTarget.HardwareAndSoftware, null);
            _ = connection.SetTitleAsync(tmp, icon.Key, SDKTarget.HardwareAndSoftware, null);

        }
    }
}
