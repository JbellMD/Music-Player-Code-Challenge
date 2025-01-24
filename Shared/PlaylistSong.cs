using System;

namespace music_manager_starter.Shared
{
    public class PlaylistSong
    {
        public Guid PlaylistId { get; set; }
        public Playlist Playlist { get; set; }
        public Guid SongId { get; set; }
        public Song Song { get; set; }
        public int Order { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
