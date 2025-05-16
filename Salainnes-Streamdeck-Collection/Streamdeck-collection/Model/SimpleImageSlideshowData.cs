using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamdeck_collection.Model
{
    internal class SimpleImageSlideshowData
    {
        public string Image { get; set; } = null;
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public int UpdateInterval { get; set; } = 10000;
        public string Url { get; set; } = "http://picsum.photos/72";
        private Guid _clientId = Guid.NewGuid();

        internal void Parse(JObject rawSettings)
        {
            var updateInterval = rawSettings["UpdateInterval"]?.ToObject<int>();
            if (updateInterval != null && updateInterval > 0)
            {
                UpdateInterval = updateInterval.Value* 1000;
            }
            var url = rawSettings["Url"]?.ToObject<string>();
            if (url != null && !string.IsNullOrWhiteSpace(url))
            {
                Url = url;
            }

            if (string.IsNullOrEmpty(Url))
            {
                Url = "http://picsum.photos/72";
            }

            if (!Url.Contains("clientid"))
            {

                if (Url.Contains("?"))
                {
                    Url += "&clientid=" + _clientId.ToString();
                }
                else
                {
                    Url += "?clientid=" + _clientId.ToString();
                }
            }
        }
    }
}
