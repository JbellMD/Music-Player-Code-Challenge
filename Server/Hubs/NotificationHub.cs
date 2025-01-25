using Microsoft.AspNetCore.SignalR;
using music_manager_starter.Shared;

namespace music_manager_starter.Server.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNewSongNotification(Song song)
        {
            var notification = new Notification
            {
                Id = song.Id,
                Title = $"New song added: {song.Title}",
                Message = $"A new song '{song.Title}' by {song.Artist} has been added.",
                NotificationType = NotificationType.NewSong
            };

            await Clients.All.SendAsync("ReceiveNotification", notification);
        }

        public async Task SendNewPlaylistNotification(Playlist playlist)
        {
            var notification = new Notification
            {
                Id = playlist.Id,
                Title = $"New playlist created: {playlist.Name}",
                Message = $"A new playlist '{playlist.Name}' has been created.",
                NotificationType = NotificationType.NewPlaylist
            };

            await Clients.All.SendAsync("ReceiveNotification", notification);
        }
    }

    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
    }

    public enum NotificationType
    {
        NewSong,
        NewPlaylist,
        SongUpdated,
        PlaylistUpdated
    }
}
