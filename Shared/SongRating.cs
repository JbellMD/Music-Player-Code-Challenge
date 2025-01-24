using System;

namespace music_manager_starter.Shared
{
    public class SongRating
    {
        public Guid Id { get; set; }
        public Guid SongId { get; set; }
        public Song Song { get; set; }
        public int Rating { get; set; }  // 1-5 stars
        public string? Comment { get; set; }
        public DateTime RatedAt { get; set; }
    }
}
