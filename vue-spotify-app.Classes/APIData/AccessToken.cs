using System;
using System.Collections.Generic;
using System.Text;

namespace vue_spotify_app.Classes.APIData
{
    public class AccessToken
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
    }
}
