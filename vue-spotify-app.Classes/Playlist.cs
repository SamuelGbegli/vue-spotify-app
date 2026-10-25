using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents a Spotify playlist.
    /// </summary>
    public class Playlist
    {
        /// <summary>
        /// The ID of the playlist on Spotify.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }
        /// <summary>
        /// The name of the playlist.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// A variant of the playlist name used for sorting.
        /// </summary>
        public string SortName { get; set; }
        /// <summary>
        /// A link to the playlist's cover image.
        /// </summary>
        public string ImageURL { get; set; } = string.Empty;
        /// <summary>
        /// The number of tracks in the playlist.
        /// </summary>
        public int NumberOfTracks { get; set; }
        /// <summary>
        /// The name of the playlist owner.
        /// </summary>
        public string OwnerName { get; set; }
        /// <summary>
        /// The ID of the playlist owner.
        /// </summary>
        public string OwnerID { get; set; }

        /// <summary>
        /// Stores when the playlist was last updated.
        /// </summary>
        public string SnapshotID { get; set; } = string.Empty;
    }
}
