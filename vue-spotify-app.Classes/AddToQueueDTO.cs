using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    public class AddToQueueDTO
    {
        public List<string> SpotifyTrackIDs { get; set; }
        public string DeviceID { get; set; }
    }
}
