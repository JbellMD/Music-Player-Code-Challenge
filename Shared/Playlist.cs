using System;
using System.Collections.Generic;

namespace music_manager_starter.Shared
{
    public class Playlist
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PlaylistSong> Songs { get; set; } = new();
    }
}
