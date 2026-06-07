using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents a batch of track view models, used to help with virtual scrolling in the frontend.
    /// </summary>
    public class TrackViewModelBatch
    {
        /// <summary>
        /// The index of the batch records found.
        /// </summary>
        public int BatchIndex { get; set; }
        /// <summary>
        /// Represents the tracks found in the batch.
        /// </summary>
        public List<TrackViewModel> TrackViewModels { get; set; } = new List<TrackViewModel>();
    }
}
