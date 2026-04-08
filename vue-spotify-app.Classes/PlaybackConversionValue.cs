using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents the result of converting a playback track name to a Spotify track identifier and possible matches.
    /// </summary>
    /// <remarks>This class is typically used to store the outcome of a track name resolution process,
    /// including the original input, the resolved Spotify ID, and any alternative track names that may correspond to
    /// the input. All properties are mutable to allow for flexible usage in various conversion scenarios.</remarks>
    public class PlaybackConversionValue
    {
        /// <summary>
        /// The date and time when the track was recorded.
        /// </summary>
        public DateTime DateRecorded { get; set; }
        /// <summary>
        /// The original track name inputted for conversion.
        /// </summary>
        public string InputtedTrackName { get; set; }
        /// <summary>
        /// If found, the Spotify track identifier corresponding to the inputted track name.
        /// </summary>
        public string SpotifyID { get; set; } = "UNKNOWN";
        /// <summary>
        /// A list of possible track names that may match the inputted track name.
        /// </summary>
        public List<PossibleTrackName> PossibleTrackNames { get; set; } = new List<PossibleTrackName>();
    }

    /// <summary>
    /// Represents a possible track name and its associated Spotify ID.
    /// </summary>
    public class PossibleTrackName
    {
        /// <summary>
        /// The name of the possible track.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The Spotify ID of the possible track.
        /// </summary>
        public string SpotifyID { get; set; }
    }
}
