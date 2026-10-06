import ArtistViewModel from "./artistViewModel";


// View model for displaying information regarding a track played on Spotify.
export default class PlaybackRecordViewModel{
  // The date and time the track was recorded as played.
  datePlayed: Date = new Date();
  // The name of the track.
  name: string = "";
  // The external URL to the track on Spotify.
  trackURL: string = "";
  // A list of artists credited with the track.
  artists: ArtistViewModel[] = [];
  // The name of the album the track belongs to.
  albumName: string = "";
  // The external URL to the album on Spotify.
  albumLink: string = "";
  // A link to the track's album cover.
  albumCover: string = "";
  // If true, means the track is in the user's Liked Songs library.
  isInLikedSongs: boolean = false;
  // The ID of the track on Spotify.
  spotifyID: string = "";

  initialiseData(data){
    this.datePlayed = new Date(data.datePlayed);
    this.name = data.name;
    this.trackURL = data.trackURL;
    this.albumName = data.albumName;
    this.albumLink = data.albumLink;
    this.albumCover = data.albumCover;
    this.isInLikedSongs = data.isInLikedSongs;
    this.spotifyID = data.spotifyID;

    data.artists.forEach(element => {
      const artist = new ArtistViewModel();
      artist.name = element.name;
      artist.externalURL = element.externalURL;
      this.artists.push(artist);
    });
  }
}
