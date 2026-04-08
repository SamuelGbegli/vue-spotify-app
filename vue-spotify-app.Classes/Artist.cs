using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents an artist's Spotify profile.
    /// </summary>
    public class Artist
    {
        /// <summary>
        /// The ID of the artist on Spotify.
        /// </summary>

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }
        /// <summary>
        /// The name of the artist.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A version of the track name used for sorting.
        /// </summary>
        public string SortName { get; set; }
        /// <summary>
        /// Spotify's internal URI for the artist's profile
        /// </summary>
        public string URI { get; set; }
        /// <summary>
        /// An external link to the artist's Spotify profile.
        /// </summary>
        public string ExternalURL { get; set; }

        public List<Track> Tracks { get; set; }

        public List<Album> Albums { get; set; }

        public Artist()
        {

        }

        public Artist(APIData.Artist apiArtist)
        {
            ID = apiArtist.id;
            Name = apiArtist.name;
            URI = apiArtist.uri;
            ExternalURL = apiArtist.external_urls.spotify;
        }
    }
    
}
