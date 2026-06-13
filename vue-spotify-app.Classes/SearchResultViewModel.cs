using System;
using System.Collections.Generic;
using System.Text;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// View model to display search results for any Spotify item type.
    /// </summary>
    public class SearchResultViewModel
    {
        /// <summary>
        /// The type of Spotify item. Can be one of the following: "album", "artist", "playlist", "track", "audiobook", "show", or "episode".
        /// </summary>
        public string ItemType { get; set; }
        /// <summary>
        /// The total number of items found for the search query.
        /// </summary>
        public int TotalItems { get; set; }
        /// <summary>
        /// The current page of results being displayed.
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// A list of items returned for the search query. The onject type will vary depending on the items returned.
        /// </summary>
        public List<object> Items { get; set; } = new List<object>();
    }
}
