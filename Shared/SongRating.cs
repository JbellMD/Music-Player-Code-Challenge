using System;

namespace music_manager_starter.Shared
{
    public sealed class SongRating
    {
        public int Id { get; set; }
        public int SongId { get; set; }
        public int Rating { get; set; }  // 1-5 stars
        public string? Comment { get; set; }
        public DateTime RatedAt { get; set; }

        // Navigation property
        public Song? Song { get; set; }
    }
}
