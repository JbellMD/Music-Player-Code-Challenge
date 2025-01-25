using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace music_manager_starter.Shared
{
    public sealed class Song
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public string? AlbumArtUrl { get; set; }
        public string? FilePath { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
        public ICollection<SongRating> Ratings { get; set; } = new List<SongRating>();

        // Computed properties
        public double AverageRating => Ratings?.Count > 0 
            ? Math.Round(Ratings.Average(r => r.Rating), 1) 
            : 0;
    }
}
