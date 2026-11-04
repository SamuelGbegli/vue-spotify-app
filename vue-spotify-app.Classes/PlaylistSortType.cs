namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Used to determine how to sort playlists.
    /// </summary>
    public enum PlaylistSortType
    {
        /// <summary>
        /// Sorts playlists by their normalised name.
        /// </summary>
        Name,
        /// <summary>
        /// Sorts playlists by their total number of tracks.
        /// </summary>
        NumberOfTracks
    }
}