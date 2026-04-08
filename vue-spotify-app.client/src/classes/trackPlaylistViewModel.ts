// Represents a summary of a playlist in a track's summary page.
export default class TrackPlaylistViewModel{
  // The ID of the playlist on Spotify.
  playlistID: string = "";
  // The name of the playlist.
  playlistName: string = "";
  // The playlist's preview image, if visable.
  image: string = "";
  /// The date and time when the track was added to the playlist.
  dateAdded: string = "";
}
