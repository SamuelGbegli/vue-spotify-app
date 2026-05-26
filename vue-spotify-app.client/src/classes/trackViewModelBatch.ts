import type TrackViewModel from "./trackViewModel";
// Represents a batch of track view models, used to help with virtual scrolling in the frontend.
export default class TrackViewModelBatch {
  // The index of the batch records found.
  batchIndex: number = 0;
  // Represents the tracks found in the batch.
  public trackViewModels: TrackViewModel[] = [];
}
