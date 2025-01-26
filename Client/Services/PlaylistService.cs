using music_manager_starter.Shared;
using System.Net.Http.Json;

namespace music_manager_starter.Client.Services
{
    public interface IPlaylistService
    {
        Task<IEnumerable<Playlist>> GetPlaylistsAsync();
        Task<Playlist?> GetPlaylistAsync(int id);
        Task<Playlist?> CreatePlaylistAsync(Playlist playlist);
        Task<Playlist?> UpdatePlaylistAsync(int id, Playlist playlist);
        Task DeletePlaylistAsync(int id);
        Task<Playlist?> AddSongToPlaylistAsync(int playlistId, int songId);
        Task<Playlist?> RemoveSongFromPlaylistAsync(int playlistId, int songId);
    }

    public class PlaylistService : IPlaylistService
    {
        private readonly IHttpService _httpService;

        public PlaylistService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<IEnumerable<Playlist>> GetPlaylistsAsync()
        {
            return await _httpService.GetAsync<IEnumerable<Playlist>>("api/playlists") ?? Array.Empty<Playlist>();
        }

        public async Task<Playlist?> GetPlaylistAsync(int id)
        {
            return await _httpService.GetAsync<Playlist>($"api/playlists/{id}");
        }

        public async Task<Playlist?> CreatePlaylistAsync(Playlist playlist)
        {
            return await _httpService.PostAsync<Playlist>("api/playlists", playlist);
        }

        public async Task<Playlist?> UpdatePlaylistAsync(int id, Playlist playlist)
        {
            return await _httpService.PutAsync<Playlist>($"api/playlists/{id}", playlist);
        }

        public async Task DeletePlaylistAsync(int id)
        {
            await _httpService.DeleteAsync($"api/playlists/{id}");
        }

        public async Task<Playlist?> AddSongToPlaylistAsync(int playlistId, int songId)
        {
            return await _httpService.PostAsync<Playlist>($"api/playlists/{playlistId}/songs/{songId}", null);
        }

        public async Task<Playlist?> RemoveSongFromPlaylistAsync(int playlistId, int songId)
        {
            return await _httpService.PutAsync<Playlist>($"api/playlists/{playlistId}/songs/{songId}/remove", null);
        }
    }
}
