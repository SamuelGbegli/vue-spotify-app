using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes.APIData
{
    public class SearchQueryResult
    {
        public TrackQueryResult Tracks { get; set; }

        public ArtistQueryResult Artists { get; set; }

        public AlbumQueryResult Albums { get; set; }

        public PlaylistQueryResult Playlists { get; set; }

        public AudiobookQueryResult Audiobooks { get; set; }

        public ShowQueryResult Shows { get; set; }

        public EpisodeQueryResult Episodes { get; set; }
    }
}
