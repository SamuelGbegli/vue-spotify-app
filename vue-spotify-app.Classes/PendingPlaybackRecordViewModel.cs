using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// View model for storing information about pending playback records.
    /// </summary>
    public class PendingPlaybackRecordViewModel
    {
        public string ID { get; set; }

        public DateTime DateAdded { get; set; }

        public string RecordedTrackName { get; set; }

        public string RecordedSpotifyID { get; set; }

        public string FoundTrackName { get; set; }

        public string FoundAlbumCover { get; set; }

        public List<Artist> FoundArtists { get; set; } = new List<Artist>();

        public string FoundAlbum { get; set; }
    }
}
