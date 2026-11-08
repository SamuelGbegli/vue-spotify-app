import TrackViewModel from "./trackViewModel";

export default class ExtendedRecordViewModel {
    ID : string = "";
    track: TrackViewModel = new TrackViewModel();
    amountPlayed: number = 0;
    reasonStart: string = "";
    reasonEnd: string = "";
    timestamp: Date | null = null;
    playbackPercentage: number = 0;
}