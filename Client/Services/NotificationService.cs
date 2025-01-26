using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using music_manager_starter.Shared;
using System;

namespace music_manager_starter.Client.Services
{
    public interface INotificationService
    {
        event Action<Notification>? OnNotificationReceived;
        event Action<string, string, string>? OnNotification;
        Task StartAsync();
        Task StopAsync();
        void ShowNotification(string message, string title, string severity);
    }

    public class NotificationService : INotificationService, IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly NavigationManager _navigationManager;
        private bool _isStarted;

        public event Action<Notification>? OnNotificationReceived;
        public event Action<string, string, string>? OnNotification;

        public NotificationService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigationManager.ToAbsoluteUri("/notificationHub"))
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<Notification>("ReceiveNotification", notification =>
            {
                OnNotificationReceived?.Invoke(notification);
                OnNotification?.Invoke(notification.Message, notification.Title, "info");
            });
        }

        public void ShowNotification(string message, string title = "Notification", string severity = "info")
        {
            var notification = new Notification
            {
                Message = message,
                Title = title,
                CreatedAt = DateTime.UtcNow
            };
            OnNotificationReceived?.Invoke(notification);
            OnNotification?.Invoke(message, title, severity);
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
