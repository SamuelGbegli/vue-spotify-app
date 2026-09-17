using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server
{
    public static class SeedTrackList
    {
        public static void SeedTrackLists(DataContext context)
        {
            var users = context.Users.ToList();
            foreach (var user in users)
            {
                var likedSongsList = context.TrackLists.FirstOrDefault(l => l.TrackListType == Classes.TrackListType.LikedSongs && l.UserID == user.SpotifyUserID);
                if (likedSongsList == null)
                {
                    likedSongsList = new Classes.TrackList
                    {
                        ID = Guid.NewGuid(),
                        Name = "Liked Songs",
                        SortName = "likedsongs",
                        TrackListType = Classes.TrackListType.LikedSongs,
                        UserID = user.SpotifyUserID
                    };
                    context.TrackLists.Add(likedSongsList);
                    context.SaveChanges();
                }
                    var userLikedSongs = context.TrackRecords.Where(st => st.UserId == user.SpotifyUserID && st.PlaylistID == null).AsQueryable();
                    int count = 0;
                    foreach (var trackRecord in userLikedSongs)
                    {
                        trackRecord.TrackListID = likedSongsList.ID;

                    };
                
                context.SaveChanges();
                var savedTracksList = context.TrackLists.FirstOrDefault(l => l.TrackListType == Classes.TrackListType.InternalList && l.UserID == user.SpotifyUserID && l.Name == "Tracks to listen to");
                if (savedTracksList == null)
                {
                    savedTracksList = new Classes.TrackList
                    {
                        ID = Guid.NewGuid(),
                        Name = "Tracks to listen to",
                        SortName = "trackstolistento",
                        TrackListType = Classes.TrackListType.InternalList,
                        UserID = user.SpotifyUserID
                    };
                    context.TrackLists.Add(savedTracksList);
                    context.SaveChanges();
                }

                if (context.SavedTracks.Any(st => st.UserID == user.ID))
                {
                    var savedTracks = context.SavedTracks.Where(st => st.UserID == user.ID).ToList();
                    count = 0;
                    foreach (var savedTrack in savedTracks)
                    {
                        var trackRecord = context.TrackRecords.FirstOrDefault(tr => tr.UserId == user.SpotifyUserID && tr.SpotifyID == savedTrack.SpotifyID && tr.TrackListID == savedTracksList.ID);
                        if(trackRecord == null)
                        {
                            var newTrackRecord = new Classes.TrackRecord
                            {
                                ID = Guid.NewGuid().ToString(),
                                SpotifyID = savedTrack.SpotifyID,
                                DateAdded = savedTrack.DateAdded,
                                UserId = user.SpotifyUserID,
                                TrackListID = savedTracksList.ID
                            };
                            context.TrackRecords.Add(newTrackRecord);

                        }
                    } 
                }
                context.SaveChanges();
            }

            var playlists = context.Playlists.ToList();
            foreach (var playlist in playlists)
            {
                if (!context.TrackLists.Any(l => l.TrackListType == Classes.TrackListType.Playlist && l.PlaylistID == playlist.ID))
                {
                    var playlistTrackList = new Classes.TrackList
                    {
                        ID = Guid.NewGuid(),
                        Name = playlist.Name,
                        SortName = playlist.SortName,
                        TrackListType = Classes.TrackListType.Playlist,
                        UserID = playlist.OwnerID,
                        PlaylistID = playlist.ID
                    };
                    context.TrackLists.Add(playlistTrackList);
                    context.SaveChanges();
                    var playlistTracks = context.TrackRecords.Where(st => st.UserId == playlist.OwnerID && st.PlaylistID == playlist.ID).AsQueryable();
                    int count = 0;
                    foreach (var trackRecord in playlistTracks)
                    {
                        trackRecord.TrackListID = playlistTrackList.ID;

                    }                    
                }
                context.SaveChanges();
            }
        }
    }
}
