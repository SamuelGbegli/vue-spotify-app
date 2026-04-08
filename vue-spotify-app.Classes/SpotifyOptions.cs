using System;
using System.Collections.Generic;
using System.Text;

namespace vue_spotify_app.Classes
{
    public class SpotifyOptions
    {
        public string ClientID { get; set; }
        public string RedirectURI { get; set; }
        public string[] Scopes { get; set; }
    }
}
