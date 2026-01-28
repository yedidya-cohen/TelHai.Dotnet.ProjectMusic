using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelHai.Dotnet.PlayerProject.Models
{
    public class MusicTrack
    {
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public List<string> Images { get; set; } = new List<string>();

        // This makes sure the ListBox shows the Name, not "MyMusicPlayer.MusicTrack"
        public override string ToString()
        {
            return string.IsNullOrEmpty(Artist) ? Title : $"{Title} - {Artist}";
        }
    }

}
