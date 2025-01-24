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
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string? AlbumArtUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public List<PlaylistSong> Playlists { get; set; } = new();
        public List<SongRating> Ratings { get; set; } = new();

        // Computed properties
        public double AverageRating => Ratings?.Count > 0 
            ? Math.Round(Ratings.Average(r => r.Rating), 1) 
            : 0;
    }
}
