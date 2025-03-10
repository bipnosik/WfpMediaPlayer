using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WfpMediaPlayer.Models
{
    public class Track
    {
        public int TrackID { get; set; }
        public string Title { get; set; }
        public int ArtistID { get; set; }
        public int? AlbumID { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Genre { get; set; }
        public string FilePath { get; set; }
        public string Format { get; set; }
        public string ArtistName { get; set; } 
        public string AlbumTitle { get; set; }
        }
}