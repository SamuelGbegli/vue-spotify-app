import Artist from "./artist";
import ArtistViewModel from "./artistViewModel";

export default class TrackViewModel {
  id: string;
  name: string;
  uri: string;
  externalURL: string;
  artists: ArtistViewModel[] = [];
  albumName: string;
  albumCover: string;
  albumUri: string;
  albumExternalURL: string;
  length: number;
  dateSaved: Date | null;
  dateLastPlayed: Date | null;
  numberOfFoundRecords: number;
  isInLikedSongs: boolean = false;
  selected: boolean = false;

  constructor(data: any) {
    this.id = data.id;
    this.name = data.name;
    this.uri = data.uri;
    this.externalURL = data.externalURL;
    data.artists.forEach((artistData: any) => {
      const artist = new ArtistViewModel();
      artist.id = artistData.id;
      artist.name = artistData.name;
      artist.uri = artistData.URI;
      artist.externalURL = artistData.externalURL;
      artist.index = artistData.index;
      this.artists.push(artist);
    });
    this.albumName = data.albumName;
    this.albumCover = data.albumCover;
    this.albumUri = data.albumURI;
    this.albumExternalURL = data.albumExternalURL;
    this.length = data.length;
    this.dateSaved = data.dateSaved ? new Date(data.dateSaved) : null;
    this.dateLastPlayed = data.dateLastPlayed? new Date(data.dateLastPlayed) : null;
    this.numberOfFoundRecords = data.numberOfFoundRecords;
    this.isInLikedSongs = data.isInLikedSongs;

    //this.artists.sort((a, b) => a.index - b.index);
  }
}
