using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Stores information about an extended playback record pulled from Spotify.
    /// </summary>
    [Table("ExtendedRecords")]
    public class ExtendedRecord
    {
        /// <summary>
        /// Identifier for the record.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        /// <summary>
        /// The URI of the track that was played.
        /// </summary>
        public string TrackURI { get; set; }
        /// <summary>
        /// The amount of time, in milliseconds, of the track that was recorded as played.
        /// </summary>
        public long AmountPlayed { get; set; }
        /// <summary>
        /// Indicates whether the track was played with shuffle enabled.
        /// </summary>
        public bool Shuffle { get; set; }
        /// <summary>
        /// Stores when the track started playing in Unix time (milliseconds or seconds). Is null for older records played while online.
        /// </summary>
        public long? OfflineTimestamp { get; set; }
        /// <summary>
        /// Describes why the track started played.
        /// </summary>
        public string ReasonStart { get; set; }
        /// <summary>
        /// Describes why the track finished playing.
        /// </summary>
        public string ReasonEnd { get; set; }
        /// <summary>
        /// Stores when the record was created.
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Stores where the track was played.
        /// </summary>
        public string Platform { get; set; }
    }
}
