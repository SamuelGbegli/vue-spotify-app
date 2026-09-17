namespace vue_spotify_app.Server.Controllers
{
    public class TrackListDTO
    {
        public Guid ListID { get; set; }
        public List<string> TrackIDs { get; set; }
    }
}