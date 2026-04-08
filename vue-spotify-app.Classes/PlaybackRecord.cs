using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents a record of a track being played in Spotify.
    /// </summary>
    public class PlaybackRecord
    {
        /// <summary>
        /// Unique identifier for the playback record.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ID { get; set; }
        /// <summary>
        /// The ID of the track on Spotify.
        /// </summary>
        public string SpotifyID { get; set; }
        /// <summary>
        /// The date and time when the track was played.
        /// </summary>
        public DateTime DatePlayed { get; set; }

        public string? Context { get; set; }

        public string? ContextURI { get; set; }
    }
}
