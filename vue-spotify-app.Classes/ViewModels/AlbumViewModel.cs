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

        /// <summary>
        /// A link to the album's cover image. Is null if no image is available.
        /// </summary>
        public string? ImageURL { get; set; }

        /// <summary>
        /// The release date of the album.
        /// </summary>
        public string ReleaseDate { get; set; }

        /// <summary>
        /// A link to the album on Spotify.
        /// </summary>
        public string ExternalURL { get; set; }
        /// <summary>
        /// The type of the album. Can be either "album", "single" or "compilation".
        /// </summary>

        public string AlbumType { get; set; }
    }
}
