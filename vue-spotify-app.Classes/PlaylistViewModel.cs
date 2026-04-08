using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// View model for playlists in Spotify.
    /// </summary>
    public class PlaylistViewModel
    {
        /// <summary>
        /// The playlist's ID.
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// The name of the playlist.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The description of the playlist.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The number of tracks in the playlist.
        /// </summary>
        public int NumberOfTracks { get; set; }
        /// <summary>
        /// The name of the account that owns the playlist.
        /// </summary>
        public string OwnerName { get; set; }
        /// <summary>
        /// A link to the owner's Spotify's account.
        /// </summary>
        public string OwnerLink { get; set; }
        /// <summary>
        /// A link to the playlist's image.
        /// </summary>
        public string ImageLink { get; set; }
        /// <summary>
        /// A link to the playlist on Spotify.
        /// </summary>
        public string ExternalURL { get; set; }
    }
}
