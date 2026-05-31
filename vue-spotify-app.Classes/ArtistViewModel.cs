using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    public class ArtistViewModel
    {
        /// <summary>
        /// The ID of the artist on Spotify.
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// The name of the artist.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Spotify's internal URI for the artist's profile.
        /// </summary>
        public string URI { get; set; }
        /// <summary>
        /// An external link to the artist's Spotify profile.
        /// </summary>
        public string ExternalURL { get; set; }
        /// <summary>
        /// Represents the order of the artist in the track's artist list. Main artists have an
        /// index of 0, with featured and supporting artists having higher indexes.
        /// </summary>
        public int Index { get; set; }
    }
}
