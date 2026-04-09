using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// A data transfer object for searching for tracks.
    /// </summary>
    public class SearchDTO
    {
        /// <summary>
        /// Filter for a track's album name.
        /// </summary>
        public string Album { get; set; } = string.Empty;
        /// <summary>
        /// Filter for a track's artist name.
        /// </summary>
        public string Artist { get; set; } = string.Empty;
        /// <summary>
        /// Filter for a track's title.
        /// </summary>
        public string Track { get; set; } = string.Empty;
        /// <summary>
        /// Filter for a track's release year. This can be a range.
        /// </summary>
        public string Year { get; set; } = string.Empty;
        /// <summary>
        /// Filter for a track's ISRC.
        /// </summary>
        public string ISRC { get; set; } = string.Empty;
        /// <summary>
        /// Filter for a track's genre.
        /// </summary>
        public string Genre { get; set; } = string.Empty;

        // The following filters are for albums only.

        /// <summary>
        /// Filter for an album's UPC.
        /// </summary>
        public string UPC { get; set; } = string.Empty;

        /// <summary>
        /// Filter for one of two album values, "new" or "hipster".
        /// </summary>
        public string Tag { get; set; } = string.Empty;

        public int Offset { get; set; }
    }
}
