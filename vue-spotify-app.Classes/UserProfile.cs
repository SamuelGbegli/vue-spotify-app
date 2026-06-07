using System;
using System.Collections.Generic;
using System.Text;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents a user's profile information retrieved from Spotify.
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// The user's Spotify ID.
        /// </summary>
        public string SpotifyID { get; set; }
        /// <summary>
        /// The user's display name on Spotify.
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// The URL of the user's profile image on Spotify.
        /// </summary>
        public string ProfileImageLink { get; set; }
    }
}
