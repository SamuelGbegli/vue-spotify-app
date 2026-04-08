using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// A temporary record for the user to review a playback record before savong to the database,
    /// </summary>
    public class PendingPlaybackRecord
    {
        /// <summary>
        /// Unique identifier for the record.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ID { get; set; }
        /// <summary>
        /// The date the record was made.
        /// </summary>
        public DateTime DateRecorded { get; set; }
        /// <summary>
        /// The name of the track recorded in the input file.
        /// </summary>
        public string InputtedName { get; set; }
        /// <summary>
        /// The Spotify track ID in the record.
        /// </summary>
        public string InputtedSpotifyID { get; set; }

        public PendingPlaybackRecord()
        {
            
        }

        public PendingPlaybackRecord(PlaybackConversionValue playbackConversionValue)
        {
            DateRecorded = playbackConversionValue.DateRecorded;
            InputtedName = playbackConversionValue.InputtedTrackName;
            InputtedSpotifyID = playbackConversionValue.SpotifyID;
        }
    }
}
