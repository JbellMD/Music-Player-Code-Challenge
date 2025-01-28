using Microsoft.JSInterop;

namespace music_manager_starter.Client.Services
{
    public interface IThemeService
    {
        bool IsDarkMode { get; set; }
        event EventHandler<bool>? ThemeChanged;
    }

    public class ThemeService : IThemeService
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _isDarkMode;
        private const string StorageKey = "theme_preference";
        private bool _isInitialized;

        public ThemeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _ = InitializeThemeAsync();
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    _ = SaveThemePreferenceAsync();
                    ThemeChanged?.Invoke(this, value);
                }
            }
        }

        public event EventHandler<bool>? ThemeChanged;

        private async Task InitializeThemeAsync()
        {
            if (_isInitialized) return;

            try
            {
                // Try to get saved preference
                var savedTheme = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
                
                if (!string.IsNullOrEmpty(savedTheme))
                {
                    _isDarkMode = savedTheme == "dark";
                }
                else
                {
                    // Check system preference
                    var prefersDark = await _jsRuntime.InvokeAsync<bool>("window.matchMedia('(prefers-color-scheme: dark)').matches");
                    _isDarkMode = prefersDark;
                }
            }
            catch
            {
                // Fallback to light theme if there's an error
                _isDarkMode = false;
            }

            _isInitialized = true;
            ThemeChanged?.Invoke(this, _isDarkMode);
        }

        private async Task SaveThemePreferenceAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, _isDarkMode ? "dark" : "light");
            }
            catch
            {
                // Ignore errors when saving preference
            }
        }
    }
}
