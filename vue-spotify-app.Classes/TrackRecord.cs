using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents a record of tracked saved to a user's library or a playlist.
    /// </summary>
    public class TrackRecord
    {
        /// <summary>
        /// Internal unique identifier for the track record.
        /// </summary>

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ID { get; set; }
        /// <summary>
        /// The ID of the track on Spotify.
        /// </summary>
        public string SpotifyID { get; set; }
        /// <summary>
        /// Gets or sets the date and time when the item was added.
        /// </summary>
        public DateTime DateAdded { get; set; }
        /// <summary>
        /// The ID of the playlist the track is saved in. Null if saved to library.
        /// </summary>
        public string? PlaylistID { get; set; }
        /// <summary>
        /// The ID of the user that saved the track to their library or playlist.
        /// </summary>
        public string UserId { get; set; }
    }
}
