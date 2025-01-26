namespace music_manager_starter.Shared
{
    public class Notification
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "info";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
