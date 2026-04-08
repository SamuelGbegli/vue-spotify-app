using System;
using System.Collections.Generic;
using System.Text;

namespace vue_spotify_app.Classes
{
    public class AuthToken
    {
        public string UserID { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime ExpirationUTC { get; set; }
    }
}
