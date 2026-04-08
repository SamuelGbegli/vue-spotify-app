// Represents an artist's Spotify profile.

export default class Artist {
    // The ID of the artist on Spotify
    id: string = "";
    // The name of the artist
    name: string = "";
    // Spotify's internal URI for the artist's profile
    uri: string = "";
    // An external link to the artist's Spotify profile
    externalUrl: string = "";

    setArtistInformation(data){
        this.id = data.id;
        this.name = data.name;
        this.uri = data.uri;
        this.externalUrl = data.external_urls.spotify
    }
}
