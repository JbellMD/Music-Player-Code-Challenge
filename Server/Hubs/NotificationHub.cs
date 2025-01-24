using Microsoft.AspNetCore.SignalR;
using music_manager_starter.Shared;

namespace music_manager_starter.Server.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(Notification notification)
        {
            await Clients.All.SendAsync("ReceiveNotification", notification);
        }

        public async Task SendNewSongNotification(Song song)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "New Song Added",
                Message = $"'{song.Title}' by {song.Artist} has been added to the library.",
                Type = NotificationType.NewSong,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await SendNotification(notification);
        }

        public async Task SendNewPlaylistNotification(Playlist playlist)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "New Playlist Created",
                Message = $"Playlist '{playlist.Name}' has been created.",
                Type = NotificationType.NewPlaylist,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await SendNotification(notification);
        }
    }
}
