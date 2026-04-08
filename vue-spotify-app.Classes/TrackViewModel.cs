using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// View model for a track on Spotify.
    /// </summary>
    public class TrackViewModel
    {
        /// <summary>
        /// The ID of the track on Spotify.
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// The name of the track.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A list of artists credited for the track.
        /// </summary>
        public List<Artist> Artists { get; set; } = new List<Artist>();
        /// <summary>
        /// The internal Spotify URI for the track.
        /// </summary>
        public string URI { get; set; }
        /// <summary>
        /// A link to the track on Spotify's website.
        /// </summary>
        public string ExternalURL { get; set; }
        /// <summary>
        /// The name of the album the track belongs to.
        /// </summary>
        public string AlbumName { get; set; }
        /// <summary>
        /// A link to the track's album cover.
        /// </summary>
        public string AlbumCover { get; set; }
        /// <summary>
        /// The internal Spotify URI for the track's album.
        /// </summary>
        public string AlbumURI { get; set; }
        /// <summary>
        /// A link to the track's album on Spotify's website.
        /// </summary>
        public string AlbumExternalURL { get; set; }
        /// <summary>
        /// The length of the track in milliseconds.
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// The date the track was saved to the user's Liked Songs library.
        /// </summary>
        public  DateTime? DateSaved { get; set; }
        /// <summary>
        /// The date the track was last recorded as played.
        /// </summary>
        public DateTime? DateLastPlayed { get; set; }
        /// <summary>
        /// If true, means the track is in the user's Liked Songs library.
        /// </summary>
        public bool IsInLikedSongs { get; set; } = true;
        /// <summary>
        /// The number of recorded times a track was played.
        /// </summary>
        public int NumberOfFoundRecords { get; set; }
    }
}
