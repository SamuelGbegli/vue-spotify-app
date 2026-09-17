using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    public class AddToPlaylistDTO
    {
        public string PlaylistID { get; set; }

        public List<string> TrackIDs { get; set; }
    }
}
