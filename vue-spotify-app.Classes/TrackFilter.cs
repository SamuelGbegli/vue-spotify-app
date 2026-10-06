using System;
using System.Collections.Generic;
using System.Text;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Class for filtering and sorting tracks in a list.
    /// </summary>
    public class TrackFilter
    {
        /// <summary>
        /// Used to return items that contains the text included.
        /// </summary>
        public string Query { get; set; }
        /// <summary>
        /// If true, applies the query to the track's name.
        /// </summary>
        public bool SearchName { get; set; }
        /// <summary>
        /// If true, applies the query to the names of the artists credited for the track.
        /// </summary>

        public bool SearchArtist { get; set; }

        /// <summary>
        /// If true, applies the query to the track album's name.
        /// </summary>
        public bool SearchAlbum { get; set; }
        /// <summary>
        /// Used to return tracks saved to a list on or after the date provided. If null,
        /// returns all tracks from the earliest known record.
        /// </summary>

        public DateTime? DateRangeFrom { get; set; }
        /// <summary>
        /// Used to return tracks saved to a list on or before the date provided. If null,
        /// returns all tracks from the latest known record.
        /// </summary>

        public DateTime? DateRangeTo { get; set; }
        /// <summary>
        /// Used to determine what to sort tracks by.
        /// </summary>

        public SortType SortType { get; set; }
        /// <summary>
        /// Used to determine whether to show items in ascending or descending order.
        /// </summary>
        public SortOrder SortOrder { get; set; }
    }
}
