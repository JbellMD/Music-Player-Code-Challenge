using music_manager_starter.Shared;

namespace music_manager_starter.Client.Services
{
    public interface IPlaylistService
    {
        Task<List<Playlist>> GetPlaylistsAsync();
        Task<Playlist> GetPlaylistAsync(Guid id);
        Task<Playlist> CreatePlaylistAsync(Playlist playlist);
        Task UpdatePlaylistAsync(Playlist playlist);
        Task DeletePlaylistAsync(Guid id);
        Task AddSongToPlaylistAsync(Guid playlistId, Guid songId);
        Task RemoveSongFromPlaylistAsync(Guid playlistId, Guid songId);
        Task ReorderPlaylistSongsAsync(Guid playlistId, List<PlaylistSong> songs);
    }

    public class PlaylistService : IPlaylistService
    {
        private readonly IHttpService _httpService;
        private const string BaseUrl = "api/playlists";

        public PlaylistService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<List<Playlist>> GetPlaylistsAsync()
        {
            return await _httpService.GetAsync<List<Playlist>>(BaseUrl) ?? new List<Playlist>();
        }

        public async Task<Playlist> GetPlaylistAsync(Guid id)
        {
            return await _httpService.GetAsync<Playlist>($"{BaseUrl}/{id}")
                ?? throw new Exception("Playlist not found");
        }

        public async Task<Playlist> CreatePlaylistAsync(Playlist playlist)
        {
            return await _httpService.PostAsync<Playlist>(BaseUrl, playlist)
                ?? throw new Exception("Failed to create playlist");
        }

        public async Task UpdatePlaylistAsync(Playlist playlist)
        {
            await _httpService.PutAsync<Playlist>($"{BaseUrl}/{playlist.Id}", playlist);
        }

        public async Task DeletePlaylistAsync(Guid id)
        {
            await _httpService.DeleteAsync($"{BaseUrl}/{id}");
        }

        public async Task AddSongToPlaylistAsync(Guid playlistId, Guid songId)
        {
            await _httpService.PostAsync<object>($"{BaseUrl}/{playlistId}/songs", songId);
        }

        public async Task RemoveSongFromPlaylistAsync(Guid playlistId, Guid songId)
        {
            await _httpService.DeleteAsync($"{BaseUrl}/{playlistId}/songs/{songId}");
        }

        public async Task ReorderPlaylistSongsAsync(Guid playlistId, List<PlaylistSong> songs)
        {
            await _httpService.PutAsync<object>($"{BaseUrl}/{playlistId}/songs/reorder", songs);
        }
    }
}
