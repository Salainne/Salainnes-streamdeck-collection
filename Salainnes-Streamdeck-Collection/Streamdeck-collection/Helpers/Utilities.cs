using Streamdeck_collection.Model;
using Streamdeck_collection.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Streamdeck_collection.Helpers
{
    internal static class Utilities
    {
        internal static string GetBase64Image(Bitmap bmp)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] errorData = ms.ToArray();
                var base64Image = "data:image/png;base64," + Convert.ToBase64String(errorData);
                return base64Image;
            }
        }

        internal static string GetRandomImageAsBase64(SimpleImageSlideshowData settings)
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
                return Helpers.Utilities.GetBase64Image(Resources.error);
            }
        }

        internal static string Post(string url, string json, out HttpStatusCode statuscode)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = client.PostAsync(url, content).Result;
                    response.EnsureSuccessStatusCode();
                    statuscode = response.StatusCode;
                    string resultat = response.Content.ReadAsStringAsync().Result;
                    return resultat;
                }
                catch (HttpRequestException ex)
                {
                    statuscode = HttpStatusCode.InternalServerError;
                    return ex.Message;
                }
            }
        }
    }
}
