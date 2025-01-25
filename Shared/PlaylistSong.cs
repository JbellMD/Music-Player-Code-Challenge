using System;

namespace music_manager_starter.Shared
{
    public sealed class PlaylistSong
    {
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public int SongId { get; set; }
        public int Order { get; set; }
        public DateTime AddedAt { get; set; }

        // Navigation properties
        public Playlist? Playlist { get; set; }
        public Song? Song { get; set; }
    }
}
