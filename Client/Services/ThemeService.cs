using System;

namespace music_manager_starter.Client.Services
{
    public interface IThemeService
    {
        bool IsDarkMode { get; set; }
        event Action<bool> OnThemeChanged;
    }

    public class ThemeService : IThemeService
    {
        private bool _isDarkMode;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnThemeChanged?.Invoke(_isDarkMode);
                }
            }
        }

        public event Action<bool> OnThemeChanged;
    }
}
