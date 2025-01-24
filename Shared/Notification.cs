using System;

namespace music_manager_starter.Shared
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public enum NotificationType
    {
        NewSong,
        NewPlaylist,
        NewRating,
        SystemMessage
    }
}
