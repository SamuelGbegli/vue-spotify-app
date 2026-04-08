import type Artist from "./artist"

export default class PendingPlaybackRecordViewModel{
  id: string = "";
  dateAdded: string = "";
  recordedTrackName: string = "";
  recordedSpotifyID: string = "";
  foundAlbumCover: string = "";
  foundTrackName: string = "";
  foundArtists: string[] = [];
  foundAlbum: string = "";
  isSelected: boolean = false;

  initialise(data){
    this.id = data.id;
    this.dateAdded = data.dateAdded;
    this.recordedTrackName = data.recordedTrackName;
    this.recordedSpotifyID = data.recordedSpotifyID;
    this.foundAlbumCover = data.foundAlbumCover;
    this.foundTrackName = data.foundTrackName;
    this.foundAlbum = data.foundAlbum;
    this.foundArtists = data.foundArtists.map(x => x.name);

  }
}
