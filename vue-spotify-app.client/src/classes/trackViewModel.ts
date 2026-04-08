import Artist from "./artist";

export default class TrackViewModel {
  id: string;
  name: string;
  uri: string;
  externalUrl: string;
  artists: Artist[] = [];
  albumName: string;
  albumCover: string;
  albumUri: string;
  albumExternalUrl: string;
  length: number;
  dateSaved: Date | null;
  dateLastPlayed: Date | null;
  numberOfFoundRecords: number;
  isInLikedSongs: boolean = false;

  constructor(data: any) {
    this.id = data.id;
    this.name = data.name;
    this.uri = data.uri;
    this.externalUrl = data.externalURL;
    data.artists.forEach((artistData: any) => {
      const artist = new Artist();
      artist.id = artistData.id;
      artist.name = artistData.name;
      artist.uri = artistData.uri;
      artist.externalUrl = artistData.externalURL;
      this.artists.push(artist);
    });
    this.albumName = data.albumName;
    this.albumCover = data.albumCover;
    this.albumUri = data.albumURI;
    this.albumExternalUrl = data.albumExternalURL;
    this.length = data.length;
    this.dateSaved = data.dateSaved ? new Date(data.dateSaved) : null;
    this.dateLastPlayed = data.dateLastPlayed? new Date(data.dateLastPlayed) : null;
    this.numberOfFoundRecords = data.numberOfFoundRecords;
    this.isInLikedSongs = data.isInLikedSongs;
  }
}
