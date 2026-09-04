using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Class to store information about a track that has been saved by a user in the application, though not necessarily in Spotify.
    /// </summary>
    public class SavedTrack
    {
        /// <summary>
        /// Identifier for the saved track.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ID { get; set; }
        /// <summary>
        /// The user that saved the track.
        /// </summary>
        public User User { get; set; }
        /// <summary>
        /// Identifier for the user that saved the track.
        /// </summary>
        public Guid UserID { get; set; }
        /// <summary>
        /// Identifier for the track in Spotify.
        /// </summary>
        public string SpotifyID { get; set; }
        /// <summary>
        /// Stores when the track was added to the database by the user.
        /// </summary>
        public DateTime DateAdded { get; set; }
    }
}
