import ArtistViewModel from "./artistViewModel";

export default class AlbumViewModel {
    id: string = "";
    name: string = "";
    artists: ArtistViewModel[] = [];
    numberOfTracks: number = 0;
    releaseDate: string = "";
    imageURL: string | null = null;
    externalURL: string = "";
    albumType: string = "";
}