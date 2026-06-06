export default class SearchDTO{
  query: string = "";
  album: string = "";
  artist: string = "";
  track: string = "";
  year: string = "";
  ISRC: string = "";
  genre: string = "";
  UPC: string = "";
  newAlbums: boolean = false;
  hipsterAlbums: boolean = false;
  itemTypes: string[] = [];

  offset: number = 0;
}
