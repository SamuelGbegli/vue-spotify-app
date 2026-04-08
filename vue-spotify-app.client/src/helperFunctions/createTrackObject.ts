import Track from "../classes/track";

// Function to construct a track object from a data fetched from the backend.
export default function createTrackObject(data): Track{
    const track = new Track();
    track.setTrack(data);
    return track;
}
