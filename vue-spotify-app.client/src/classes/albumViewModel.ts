import ArtistViewModel from "./artistViewModel";

export default class AlbumViewModel {
    id: string = "";
    name: string = "";
    artists: ArtistViewModel[] = [];
    numberOfTracks: number = 0;
}