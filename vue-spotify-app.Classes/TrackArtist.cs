using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents mappings between a track and the arists credited.
    /// </summary>
    public class TrackArtist
    {
        /// <summary>
        /// Unique identifier for the object
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// The Spotify ID for the track.
        /// </summary>
        public string TrackID { get; set; }
        /// <summary>
        /// The track associated with this object.
        /// </summary>
        public Track Track { get; set; }

        /// <summary>
        /// The Spotify ID for the artist.
        /// </summary>
        public string ArtistID { get; set; }
        /// <summary>
        /// The artist associated with this object.
        /// </summary>
        public Artist Artist { get; set; }
        /// <summary>
        /// Represents the order of the artist in the track's artist list. Main artists have an
        /// index of 0, with featured and supporting artists having higher indexes.
        /// </summary>
        public int Index { get; set; }
    }
}
