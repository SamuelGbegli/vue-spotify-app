using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents a user that can access the application.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user, generated when a user is added.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        /// <summary>
        /// The user's Spotify ID.
        /// </summary>
        public string SpotifyUserID { get; set; } = "";
        /// <summary>
        /// The user's display name.
        /// </summary>
        public string DisplayName { get; set; } = "";

        public SpotifyToken? SpotifyToken { get; set; }
    }
}
