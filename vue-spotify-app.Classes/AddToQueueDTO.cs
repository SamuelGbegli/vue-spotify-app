using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Data transfer object to add tracks a Spotify queue.
    /// </summary>
    public class AddToQueueDTO
    {
        /// <summary>
        /// A list of Spotify track IDs to be added to the queue.
        /// </summary>
        public List<string> SpotifyTrackIDs { get; set; }
        /// <summary>
        /// The ID of the device to add tracks to.
        /// </summary>
        public string DeviceID { get; set; }
    }
}
