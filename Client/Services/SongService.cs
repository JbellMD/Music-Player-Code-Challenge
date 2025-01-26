using Microsoft.AspNetCore.Components.Forms;
using music_manager_starter.Shared;
using System.Net.Http.Json;

namespace music_manager_starter.Client.Services
{
    public interface ISongService
    {
        Task<IEnumerable<Song>> GetSongsAsync(string? searchTerm = null);
        Task<Song?> GetSongAsync(int id);
        Task<Song?> CreateSongAsync(Song song, IBrowserFile file);
        Task<Song?> UpdateSongAsync(int id, Song song);
        Task DeleteSongAsync(int id);
        Task<SongRating?> RateSongAsync(int id, int rating);
    }

    public class SongService : ISongService
    {
        private readonly IHttpService _httpService;
        private readonly HttpClient _httpClient;

        public SongService(IHttpService httpService, HttpClient httpClient)
        {
            _httpService = httpService;
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Song>> GetSongsAsync(string? searchTerm = null)
        {
            var query = string.IsNullOrEmpty(searchTerm) ? "" : $"?search={searchTerm}";
            return await _httpService.GetAsync<IEnumerable<Song>>($"api/songs{query}") ?? Array.Empty<Song>();
        }

        public async Task<Song?> GetSongAsync(int id)
        {
            return await _httpService.GetAsync<Song>($"api/songs/{id}");
        }

        public async Task<Song?> CreateSongAsync(Song song, IBrowserFile file)
        {
            // First upload the file
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "file", file.Name);

            var response = await _httpClient.PostAsync("api/songs/upload", content);
            if (!response.IsSuccessStatusCode)
                return null;

            var filePath = await response.Content.ReadAsStringAsync();
            song.FilePath = filePath;

            // Then create the song
            return await _httpService.PostAsync<Song>("api/songs", song);
        }

        public async Task<Song?> UpdateSongAsync(int id, Song song)
        {
            return await _httpService.PutAsync<Song>($"api/songs/{id}", song);
        }

        public async Task DeleteSongAsync(int id)
        {
            await _httpService.DeleteAsync($"api/songs/{id}");
        }

        public async Task<SongRating?> RateSongAsync(int id, int rating)
        {
            return await _httpService.PostAsync<SongRating>($"api/songs/{id}/rate", new { Rating = rating });
        }
    }
}
