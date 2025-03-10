using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WfpMediaPlayer.Models
{
    public class PlaylistTrack
    {
        public int PlaylistID { get; set; }
        public int TrackID { get; set; }
        public int TrackOrder { get; set; } = 1; // Значение по умолчанию
    }
}