using music_manager_starter.Shared;

namespace music_manager_starter.Client.Services
{
    public interface INotificationService
    {
        event Action<string, string, string>? OnNotification;
        void ShowNotification(string message, string title = "Notification", string severity = "info");
    }
}
