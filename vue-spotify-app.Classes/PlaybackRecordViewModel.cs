using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// View model for displaying information regarding a track played on Spotify.
    /// </summary>
    public class PlaybackRecordViewModel
    {
        /// <summary>
        /// The date and time the track was recorded as played.
        /// </summary>
        public DateTime DatePlayed { get; set; }
        /// <summary>
        /// The name of the track.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The external URL to the track on Spotify.
        /// </summary>
        public string TrackURL { get; set; }
        /// <summary>
        /// A list of artists credited with the track.
        /// </summary>
        public List<Artist> Artists { get; set; } = new List<Artist>();
        /// <summary>
        /// The name of the album the track belongs to.
        /// </summary>
        public string AlbumName { get; set; }
        /// <summary>
        /// The external URL to the albun on Spotify.
        /// </summary>
        public string AlbumLink { get; set; }
        /// <summary>
        /// A link to the track's album cover.
        /// </summary>
        public string AlbumCover { get; set; }
        /// <summary>
        /// If true, means the track is in the user's Liked Songs library.
        /// </summary>
        public bool IsInLikedSongs { get; set; }
        /// <summary>
        /// The ID of the track on Spotify.
        /// </summary>
        public string SpotifyID { get; set; }
    }
}
