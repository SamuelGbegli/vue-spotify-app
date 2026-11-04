namespace vue_spotify_app.Server.Controllers
{
    /// <summary>
    /// Data transfer object for an internal Track list and corresponding track IDs.
    /// </summary>
    public class TrackListDTO
    {
        /// <summary>
        /// The ID of the track list.
        /// </summary>
        public Guid ListID { get; set; }
        /// <summary>
        /// A list of Spotify track IDs.
        /// </summary>
        public List<string> TrackIDs { get; set; }
    }
}