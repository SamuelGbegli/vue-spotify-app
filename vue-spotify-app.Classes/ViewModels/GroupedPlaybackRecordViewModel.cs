using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    public class GroupedPlaybackRecordViewModel
    {
        /// <summary>
        /// The Spotify ID for the track.
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// The name of the track.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A link to the external URL for the track.
        /// </summary>
        public string ExternalURL { get; set; }
        /// <summary>
        /// The total number of records found for the track.
        /// </summary>
        public int NumberOfFoundRecords { get; set; }
        /// <summary>
        /// A link to the track's album cover.
        /// </summary>
        public string AlbumCoverURL { get; set; }
    }
}
