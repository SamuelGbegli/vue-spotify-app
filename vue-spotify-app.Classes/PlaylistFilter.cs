using System;
using System.Collections.Generic;
using System.Text;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// A filter used for searching and sorting playlists.
    /// </summary>
    public class PlaylistFilter
    {
        /// <summary>
        /// The page number of playlists to return.
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// The number of playlists to return per page.
        /// </summary>
        public int NumberOfPlaylists { get; set; }
        /// <summary>
        /// A search query to filter playlists by name.
        /// </summary>
        public string Query { get; set; }
        /// <summary>
        /// If true, only return playlists that are made by the user.
        /// </summary>
        public bool ReturnUserPlaylistsOnly { get; set; }
        /// <summary>
        /// Determines how to sort the playlists returned by the database.
        /// </summary>
        public PlaylistSortType SortType { get; set; }
        /// <summary>
        /// Determines whether to return playlists in ascending or descending order.
        /// </summary>
        public SortOrder SortOrder { get; set; }
    }
}
