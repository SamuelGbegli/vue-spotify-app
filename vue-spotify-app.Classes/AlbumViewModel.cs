using System;
using System.Collections.Generic;
using System.Text;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// View model for repesenting an album.
    /// </summary>
    public class AlbumViewModel
    {
        /// <summary>
        /// The Spotify ID for the album.
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// The name of the album.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A list of artists credited for the album.
        /// </summary>
        public List<ArtistViewModel> Artists { get; set; }
        /// <summary>
        /// The number of tracks in the album.
        /// </summary>
        public int NumberOfTracks { get; set; }
    }
}
