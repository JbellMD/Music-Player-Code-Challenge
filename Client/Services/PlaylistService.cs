using music_manager_starter.Shared;
using System.Net.Http.Json;

namespace music_manager_starter.Client.Services
{
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

        public async Task<Playlist> GetPlaylistAsync(int id)
        {
            return await _httpService.GetAsync<Playlist>($"api/playlists/{id}") ?? 
                   throw new Exception($"Playlist with id {id} not found");
        }

        public async Task<Playlist> CreatePlaylistAsync(Playlist playlist)
        {
            return await _httpService.PostAsync<Playlist>("api/playlists", playlist) ?? 
                   throw new Exception("Failed to create playlist");
        }

        public async Task<Playlist> UpdatePlaylistAsync(Playlist playlist)
        {
            return await _httpService.PutAsync<Playlist>($"api/playlists/{playlist.Id}", playlist) ?? 
                   throw new Exception($"Failed to update playlist with id {playlist.Id}");
        }

        public async Task DeletePlaylistAsync(int id)
        {
            await _httpService.DeleteAsync($"api/playlists/{id}");
        }

        public async Task AddSongToPlaylistAsync(int playlistId, int songId)
        {
            await _httpService.PostAsync<object>($"api/playlists/{playlistId}/songs/{songId}", new { });
        }

        public async Task RemoveSongFromPlaylistAsync(int playlistId, int songId)
        {
            await _httpService.DeleteAsync($"api/playlists/{playlistId}/songs/{songId}");
        }
    }
}
