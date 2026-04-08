using System;
using System.Collections.Generic;
using System.Text;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents a summary of a playlist in a track's summary page.
    /// </summary>
    public class TrackPlaylistViewModel
    {
        /// <summary>
        /// The ID of the playlist on Spotify.
        /// </summary>
        public string PlaylistID { get; set; }
        /// <summary>
        /// The name of the playlist.
        /// </summary>
        public string PlaylistName { get; set; }
        /// <summary>
        /// The playlist's preview image, if visable.
        /// </summary>
        public string? Image { get; set; }
        /// <summary>
        /// The date and time when the track was added to the playlist.
        /// </summary>
        public DateTime DateAdded { get; set; }
    }
}
