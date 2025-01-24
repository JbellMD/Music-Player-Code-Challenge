using Microsoft.AspNetCore.SignalR.Client;
using music_manager_starter.Shared;

namespace music_manager_starter.Client.Services
{
    public interface INotificationService
    {
        event Action<Notification>? OnNotificationReceived;
        Task StartAsync();
        Task StopAsync();
    }

    public class NotificationService : INotificationService, IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private bool _isStarted;

        public event Action<Notification>? OnNotificationReceived;

        public NotificationService(NavigationManager navigationManager)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri("/notificationHub"))
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<Notification>("ReceiveNotification", notification =>
            {
                OnNotificationReceived?.Invoke(notification);
            });
        }

        public async Task StartAsync()
        {
            if (!_isStarted)
            {
                await _hubConnection.StartAsync();
                _isStarted = true;
            }
        }

        public async Task StopAsync()
        {
            if (_isStarted)
            {
                await _hubConnection.StopAsync();
                _isStarted = false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
