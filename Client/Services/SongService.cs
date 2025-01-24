using music_manager_starter.Shared;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

namespace music_manager_starter.Client.Services
{
    public interface ISongService
    {
        Task<List<Song>> GetSongsAsync(string? search = null);
        Task<Song> GetSongAsync(Guid id);
        Task<Song> CreateSongAsync(Song song);
        Task UpdateSongAsync(Song song);
        Task DeleteSongAsync(Guid id);
        Task<SongRating> RateSongAsync(Guid songId, SongRating rating);
        Task<string> UploadAlbumArtAsync(Guid songId, IBrowserFile file);
    }

    public class SongService : ISongService
    {
        private readonly HttpClient _httpClient;

        public SongService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Song>> GetSongsAsync(string? searchTerm = null)
        {
            var url = "api/songs";
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                url += $"?search={Uri.EscapeDataString(searchTerm)}";
            }
            return await _httpClient.GetFromJsonAsync<List<Song>>(url) ?? new List<Song>();
        }

        public async Task<Song> GetSongAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Song>($"api/songs/{id}") 
                ?? throw new Exception("Song not found");
        }

        public async Task<Song> CreateSongAsync(Song song)
        {
            var response = await _httpClient.PostAsJsonAsync("api/songs", song);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Song>() ?? throw new Exception("Failed to create song");
        }

        public async Task<Song> AddSongAsync(Song song)
        {
            var response = await _httpClient.PostAsJsonAsync("api/songs", song);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Song>() ?? throw new Exception("Failed to add song");
        }

        public async Task UpdateSongAsync(Song song)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/songs/{song.Id}", song);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Song>() ?? throw new Exception("Failed to update song");
        }

        public async Task DeleteSongAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/songs/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<SongRating> RateSongAsync(Guid songId, SongRating rating)
        {
            return await _httpClient.PostAsJsonAsync<SongRating>($"api/songs/{songId}/rate", rating) 
                ?? throw new Exception("Failed to rate song");
        }

        public async Task<string> UploadAlbumArtAsync(Guid songId, IBrowserFile file)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            content.Add(fileContent, "file", file.Name);

            var response = await _httpClient.PostAsync($"api/songs/{songId}/upload-art", content);
            return response.Content.ReadAsStringAsync().Result ?? throw new Exception("Failed to upload album art");
        }
    }
}
