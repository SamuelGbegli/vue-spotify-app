using System;
using System.Collections.Generic;
using System.Text;

namespace vue_spotify_app.Classes
{
    public class PlaylistFilter
    {
        public int Page { get; set; }
        public int NumberOfPlaylists { get; set; }
        public string Query { get; set; }
        public bool ReturnUserPlaylistsOnly { get; set; }
        public PlaylistSortType SortType { get; set; }
        public SortOrder SortOrder { get; set; }
    }
}
