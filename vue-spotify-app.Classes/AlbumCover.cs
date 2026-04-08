using System.ComponentModel.DataAnnotations.Schema;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents a cover for an album.
    /// </summary>
    public class AlbumCover
    {
        /// <summary>
        /// An internal unique identifier for the album cover.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ID { get; set; }
        /// <summary>
        /// A direct link to the album cover hosted on Spotify.
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// The width of the album cover.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The height of the album cover.
        /// </summaryg
        public int Height { get; set; }

        /// <summary>
        /// The ID of the album this cover belongs to.
        /// </summary>
        public string AlbumID { get; set; }

        /// <summary>
        /// The album this cover belongs to.
        /// </summary>
        public Album Album { get; set; }

        public AlbumCover()
        {

        }

        public AlbumCover(APIData.Image image)
        {
            Link = image.url;
            Width = (int)image.width;
            Height = (int)image.height;
        }
    }
}
