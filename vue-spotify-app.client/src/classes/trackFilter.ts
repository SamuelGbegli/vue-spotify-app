import SortOrder from "@/enumClasses/sortOrder";
import SortType from "@/enumClasses/sortType";

export default class TrackFilter {

  query: string = "";
  searchName: boolean = true;
  searchArtist: boolean = true;
  searchAlbum: boolean = true;
  dateRangeFrom: string | null = null;
  dateRangeTo: string | null = null;
  sortType: number = SortType.Name;
  sortOrder: number = SortOrder.Ascending;
}
