using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamdeck_collection.Model
{
    internal class ImageTestData
    {
        public string Image { get; set; } = null;
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public int UpdateInterval { get; set; } = 1000;
    }
}
