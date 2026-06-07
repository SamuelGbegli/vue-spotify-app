export default class AddToPlaylistDTO {
  playlistId: string;
  trackUris: string[];

  constructor(playlistId: string, trackUris: string[]) {
    this.playlistId = playlistId;
    this.trackUris = trackUris;
  }
}
