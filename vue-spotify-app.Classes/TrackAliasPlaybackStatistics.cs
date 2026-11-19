using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Class for storing statistics relating to aliased tracks' playback records.
    /// </summary>
    public class TrackAliasPlaybackStatistics
    {
        /// <summary>
        /// Unique identifier for the class.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        public Guid UserID { get; set; }
        /// <summary>
        /// The ID of the track alias the statistics are for.
        /// </summary>
        public Guid TrackAliasID { get; set; }
        /// <summary>
        /// The number of times a track has been recorded as played. Used for faster sorting and filtering.
        /// </summary>
        public long TotalRecordedPlays { get; set; }
        /// <summary>
        /// The last time the track was recorded as played.
        /// </summary>
        public DateTime? LastRecordedPlayDate { get; set; }
        /// <summary>
        /// When the record was last updated.
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}
