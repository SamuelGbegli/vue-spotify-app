using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vue_spotify_app.Classes.APIData
{
    public class Item
    {
        public string added_at { get; set; }
        public Owner? added_by { get; set; }
        public string played_at { get; set; }
        public Track track { get; set; }
        public Context context { get; set; }
    }
}
