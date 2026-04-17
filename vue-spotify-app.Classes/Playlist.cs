using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes
{
    public class Playlist
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SortName { get; set; }
        public string ImageURL { get; set; } = string.Empty;
         public int NumberOfTracks { get; set; }
        public string OwnerName { get; set; }
        public string OwnerID { get; set; }

        public string SnapshotID { get; set; } = string.Empty;
    }
}
