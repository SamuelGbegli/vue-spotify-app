using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Data transfer object to add tracks to a playlist.
    /// </summary>
    public class AddToPlaylistDTO
    {
        /// <summary>
        /// The ID of the playlist tracks will be added to.
        /// </summary>
        public string PlaylistID { get; set; }

        /// <summary>
        /// Contains a list of Spotify track IDs to be added to the playlist.
        /// </summary>
        public List<string> TrackIDs { get; set; }
    }
}
