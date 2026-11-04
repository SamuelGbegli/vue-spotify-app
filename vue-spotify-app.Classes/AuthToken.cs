using System;
using System.Collections.Generic;
using System.Text;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents an authentication token for a Spotify user.
    /// </summary>
    public class AuthToken
    {
        /// <summary>
        /// The Spotify ID of the user the token belongs to.
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// The access token. Used to authenticate API requests.
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// The refresh token. Used to obtain a new access token when the current one expires.
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// Stores the date and time the access token will expire.
        /// </summary>
        public DateTime ExpirationUTC { get; set; }
    }
}
