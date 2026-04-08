// Represents an album saved on Spotify

import Artist from "./artist"
import AlbumCover from "./albumCover";

export default class Album{
    // The Spotify ID for the album
    id: string = "";
    // The name of the album
    name: string = "";
    // The number of tracks the album has
    numberOfTracks: number = 0;
    // The date the album was released
    releaseDate: Date = new Date();
    // An interal URI for the album on Spotify
    spotifyUri: string = "";
    // An external URL to view the album on Spotify's website
    externalUrl: string = ""
    // Stores the album's cover. Can be null
    albumCover: AlbumCover | null = null
    // An array of the artists credited for the album
    artists: Artist[] = []

    setAlbum(data){
        this.id = data.id;
        this.name = data.name;
        this.numberOfTracks = data.total_tracks;
        this.releaseDate = new Date(data.release_date);
        this.spotifyUri = data.uri;
        this.externalUrl = data.external_urls.spotify;

        if(data.images.length > 0) {
            this.albumCover = new AlbumCover();
            this.albumCover.generateAlbumCover(data.images[0]);
        }

        data.artists.forEach(element => {
            const artist = new Artist();
            artist.setArtistInformation(element);
            this.artists.push(artist);
        });
    }   
}