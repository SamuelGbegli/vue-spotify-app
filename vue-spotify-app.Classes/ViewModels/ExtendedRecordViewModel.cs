using System;
using System.Collections.Generic;
using System.Text;

namespace vue_spotify_app.Classes
{
    public class ExtendedRecordViewModel
    {
        /// <summary>
        /// Identifer for the parent record.
        /// </summary>
        public Guid ID { get; set; }
        /// <summary>
        /// View model for the track the record represents.
        /// </summary>
        public TrackViewModel Track { get; set; }
        /// <summary>
        /// The amount of time, in milliseconds, that the track was played for before ending.
        /// </summary>
        public long AmountPlayed { get; set; }
        /// <summary>
        /// Gives the reason why a track was played.
        /// </summary>
        public string ReasonStart { get; set; }
        /// <summary>
        /// Gives the reason why the track stopped playing.
        /// </summary>
        public string ReasonEnd { get; set; }
        /// <summary>
        /// Shows when the track was saved to Spotify's database.
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Represents the ratio between the track's length and how much was recorded as played.
        /// </summary>
        public double PlaybackPercentage
        {
            get
            {
                if (Track == null || Track.Length == 0)
                    return 0;
                return (double)AmountPlayed / Track.Length * 100;
            }
        }
    }
}
