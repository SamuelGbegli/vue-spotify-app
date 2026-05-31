using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vue_spotify_app.Classes.SortNameHelpers;

namespace vue_spotify_app.Classes
{
    /// <summary>
    /// Represents a track in Spotify.
    /// </summary>
    public class Track
    {
        /// <summary>
        /// The ID of the track in Spotify.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }
        /// <summary>
        /// The name of the track.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A version of the track name used for sorting.
        /// </summary>
        public string SortName { get; set; } = string.Empty;
        /// <summary>
        /// A list of the artists credited for the track.
        /// </summary>
        public List<Artist> Artists { get; set; } = new List<Artist>();
        /// <summary>
        /// A version of the name of the lead artist credited for the track. Used for sorting prposes.
        /// </summary>
        public string ArtistSortName { get; set; } = string.Empty;
        /// <summary>
        /// The ID of the album the track belongs to.
        /// </summary>
        public string AlbumID { get; set; }
        /// <summary>
        /// The album the track belongs to.
        /// </summary>
        public Album Album { get; set; }
        /// <summary>
        /// A version of the track's album name used for sorting.
        /// </summary>
        public string AlbumSortName { get; set; }
        /// <summary>
        /// Spotify's interal URI for the track.
        /// </summary>
        public string SpotifyURI { get; set; }
        /// <summary>
        /// A link to the track on Spotify's website.
        /// </summary>
        public string ExternalURL { get; set; }
        /// <summary>
        /// The length of the track in milliseconds.
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// If true, means the track has been flagged to contain vulgar or offensive language.
        /// </summary>
        public bool Explicit { get; set; }
        /// <summary>
        /// Represents a unique identifier for the track, which is used across multiple platforms.
        /// </summary>
        public string? ISRC { get; set; }
        /// <summary>
        /// If true, means the track can be played on Spotify.
        /// </summary>
        public bool Playable { get; set; }
        /// <summary>
        /// Stores a key used for matching purposes.
        /// </summary>
        public string MatchKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? AliasID { get; set; }
        /// <summary>
        /// Represents an alias for the track if there are deemed to be more than one version of the same recording.
        /// </summary>
        public TrackAlias? Alias { get; set; }

        /// <summary>
        /// Temporay list of tracks
        /// </summary>
        public static List<Track> Tracks = new List<Track>();

        public Track()
        {
            
        }

        public Track(APIData.Track track)
        {
            ID = track.id;
            Name = track.name;
            SortName = RegexHelpers.GenerateSortName(track.name);
            Artists = track.artists.Select(a => new Artist(a)).ToList();
            Album = new Album(track.album);
            AlbumID = track.album.id;
            SpotifyURI = track.uri;
            ExternalURL = track.external_urls.spotify;
            Length = track.duration_ms;
            Explicit = track.@explicit;
            ISRC = track.external_ids.isrc;
            Playable = track.is_playable;
        }

        public void GenerateMatchKey()
        {
            var normalisedName = RegexHelpers.GenerateSortName(Name, true);
            var artistIDs = Artists.Select(a => a.ID).ToList();
            var lengthBucket = Length / 2000;
            MatchKey = $"{normalisedName}|{string.Join("",artistIDs)}|{lengthBucket}";
        }
    }
}
