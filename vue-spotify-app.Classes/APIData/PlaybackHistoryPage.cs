using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes.APIData
{
    public class PlaybackHistoryPage
    {
        public string href { get; set; }
        public int limit { get; set; }
        public string next { get; set; }
        public Cursors cursors { get; set; }
        public int total { get; set; }
        public List<Item> items { get; set; }
    }
}
