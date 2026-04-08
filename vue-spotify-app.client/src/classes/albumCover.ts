// Represents a cover for an album.

export default class AlbumCover{
    // A direct link to the album cover hosted on Spotify
    link: string = "";
    // The width of the album cover
    width: number = 0;
    // The height of the album cover
    height: number = 0;

    // Get an album cover from a returned API value
    generateAlbumCover(value) {
        this.link = value.url;
        this.width = value.width;
        this.height = value.height;
    }
}