using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents an album saved on Spotify.
    /// </summary>
    public class Album
    {
        /// <summary>
        /// The Spotify ID for the album.
        /// </summary> 

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }
        /// <summary>
        /// The name of the album.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A version of the track name used for sorting.
        /// </summary>
        public string SortName { get; set; }
        /// <summary>
        /// The number of tracks the album has.
        /// </summary>
        public int NumberOfTracks { get; set; }
        /// <summary>
        /// The date the album was released.
        /// </summary>
        /// 
        public string ReleaseDate { get; set; }
        /// <summary>
        /// The known precision of the release date. Can be year, month or day.
        /// </summary>
        public string ReleaseDatePrecision { get; set; }
        /// <summary>
        /// A list of tracks that belong to the album.
        /// </summary>
        public List<Track> Tracks { get; set; }
        /// <summary>
        /// An external link to the artist's Spotify profile.
        /// </summary>
        public string SpotifyURI { get; set; }
        /// <summary>
        /// An external URL to view the album on Spotify's website.
        /// </summary>
        public string ExternalURL { get; set; }
        /// <summary>
        /// Stores the album's cover. Can be null.
        /// </summary>
        public AlbumCover? AlbumCover { get; set; }
        /// <summary>
        /// A list of artists credited for the album.
        /// </summary>
        public List<Artist> Artists { get; set; }
        /// <summary>
        /// The type of album. Can be album, single or compilation.
        /// </summary>
        public string AlbumType { get; set; }

        public Album()
        {
            
        }

        public Album(APIData.Album album)
        {
            ID = album.id;
            Name = album.name;
            NumberOfTracks = album.total_tracks;
            ReleaseDate = album.release_date;
            ReleaseDatePrecision = album.release_date_precision;
            SpotifyURI = album.uri;
            ExternalURL = album.external_urls.spotify;
            AlbumType = album.album_type;
            if (album.images != null && album.images.Count > 0)
            {
                AlbumCover = new AlbumCover(album.images.First());
            }
            Artists = new List<Artist>();
            foreach (var apiArtist in album.artists)
            {
                Artists.Add(new Artist(apiArtist));
            }
        }
    }
}
