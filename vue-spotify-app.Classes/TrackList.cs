using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Class for representing a list of tracks, whether saved in Spotify (Liked Songs or playlists) or within the application.
    /// </summary>
    public class TrackList
    {
        /// <summary>
        /// Identifier for the class.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        /// <summary>
        /// The name of the list.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A modified version of the list's name used for sorting purposes.
        /// </summary>
        public string SortName { get; set; }
        /// <summary>
        /// The ID of the user that made the list.
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// Describes what the list represents. This includes a Liked Songs library, a Spotify playlist or an internal list.
        /// </summary>
        public TrackListType TrackListType { get; set; }
        /// <summary>
        /// The ID of the playlist the list refers to. Can be null.
        /// </summary>
        public string? PlaylistID { get; set; }
        /// <summary>
        /// Represents the tracks in the list.
        /// </summary>
        public List<TrackRecord> Tracks { get; set; }
        /// <summary>
        /// The date and time when the list was created.
        /// </summary>
        public DateTime? DateCreated { get; set; }
        /// <summary>
        /// The date and time when the list was last modified.
        /// </summary>
        public DateTime? DateModified { get; set; }
    }
}
