using Newtonsoft.Json.Linq;
using streamdeck_client_csharp.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamdeck_collection.Model
{
    public class ActiveIconContainer
    {
        public WillAppearEvent WillAppearEvent { get; set; }
        public object Data { get; set; }
        public JObject RawSettings { get { return _settings; } set { _settings = value; HasNewSettings = true; } }
        private JObject _settings;
        public bool HasNewSettings { get; set; } = false;
    }
}
