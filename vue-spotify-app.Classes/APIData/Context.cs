using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes.APIData
{
    public class Context
    {
        public string type { get; set; }
        public string href { get; set; }
        public ExternalUrls external_urls { get; set; }
        public string uri { get; set; }
    }
}
