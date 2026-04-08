using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Used to store information about tracks deemed to be the same, even if IDs differ. This does not include tracks deemed to be modifications of the original (e.g.., edits, covers, remixes).
    /// </summary>
    public class TrackAlias
    {
        /// <summary>
        /// Unique identifier for the class.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        /// <summary>
        /// An identifing name for the track.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Stores any Spotify tracks that are deemed to be the same.
        /// </summary>
        public List<Track> Tracks { get; set; } = new List<Track>();
    }
}
