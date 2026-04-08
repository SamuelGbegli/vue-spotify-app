using System;
using System.Collections.Generic;
using System.Text;

namespace vue_spotify_app.Classes
{
    public class TrackFilter
    {
        public string Query { get; set; }

        public bool SearchName { get; set; }

        public bool SearchArtist { get; set; }

        public bool SearchAlbum { get; set; }

        public DateTime? DateRangeFrom { get; set; }

        public DateTime? DateRangeTo { get; set; }

        public SortType SortType { get; set; }

        public SortOrder SortOrder { get; set; }
    }
}
