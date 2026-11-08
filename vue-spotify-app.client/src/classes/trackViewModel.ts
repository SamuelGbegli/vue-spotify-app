import Artist from "./artist";
import ArtistViewModel from "./artistViewModel";

export default class TrackViewModel {
  id: string = "";
  name: string = "";
  uri: string = "";
  externalURL: string = "";
  artists: ArtistViewModel[] = [];
  albumName: string = "";
  albumCover: string = "";
  albumUri: string = "";
  albumExternalURL: string = "";
  length: number = 0;
  dateSaved: Date | null = null;
  dateLastPlayed: Date | null = null;
  numberOfFoundRecords: number = 0;
  isInLikedSongs: boolean = false;
  selected: boolean = false;
  index: number = 0;

}
