import PlaylistSortType from "@/enumClasses/playlistSortType";
import SortOrder from "@/enumClasses/sortOrder";

export default class PlaylistFilter{
    query: string = "";
    returnUserPlaylistsOnly: boolean = false;
    
  sortType: number = PlaylistSortType.Name;
  sortOrder: number = SortOrder.Ascending;
}