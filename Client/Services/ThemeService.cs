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

        public ThemeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            InitializeTheme().ConfigureAwait(false);
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    SaveThemePreference().ConfigureAwait(false);
                    ThemeChanged?.Invoke(this, value);
                }
            }
        }

        public event EventHandler<bool>? ThemeChanged;

        private async Task InitializeTheme()
        {
            try
            {
                // Try to get saved preference
                var savedTheme = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
                
                if (savedTheme != null)
                {
                    _isDarkMode = savedTheme == "dark";
                }
                else
                {
                    // Check system preference
                    _isDarkMode = await _jsRuntime.InvokeAsync<bool>("window.matchMedia", "(prefers-color-scheme: dark)").Match;
                }
                
                ThemeChanged?.Invoke(this, _isDarkMode);
            }
            catch
            {
                // Fallback to light theme if there's an error
                _isDarkMode = false;
            }
        }

        private async Task SaveThemePreference()
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
