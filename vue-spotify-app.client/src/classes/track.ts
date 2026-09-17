// Represents a track in Spotify

import Artist from "./artist"
import Album from "./album"

export default class Track {
    // The ID of the track in Spotify
    id: string = "";
    // The name of the track
    name: string = "";
    // An array of the artists credited for the track
    artists: Artist[] = []
    // The album the track belongs to
    album: Album = new Album()
    // Spotify's internal URI for the track
    spotifyUri: string = ""
    // A link to the track on Spotify's website
    externalURL: string = ""
    // The length of the track in milliseconds
    length: number = 0;
    // If true, means the track has been flagged to contain vulgar or offensive language
    explicit: boolean = false;
    // A metric on Spotify determining the track's popularity at the time the track it was fetched
    popularity: number = 0
    // The date the track was saved to the user's library or playlist (if applicable)
    dateSaved?: Date = undefined;
    setTrack(data){
        this.id = data.id;
        this.name = data.name;
        this.spotifyUri = data.uri;
        this.externalURL = data.external_urls.spotify;
        this.length = data.duration_ms;
        this.explicit = data.explicit;
        this.popularity = data.popularity;

        data.artists.forEach(element => {
                    const artist = new Artist();
                    artist.setArtistInformation(element);
                    this.artists.push(artist);
                });

        this.album.setAlbum(data.album);
    }
}
