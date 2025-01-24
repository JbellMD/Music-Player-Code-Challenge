using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using music_manager_starter.Shared;

namespace music_manager_starter.Client.Services
{
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

        public async Task<Song> AddSongAsync(Song song)
        {
            var response = await _httpClient.PostAsJsonAsync("api/songs", song);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Song>() ?? throw new Exception("Failed to add song");
        }

        public async Task<Song> UpdateSongAsync(Song song)
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

        public async Task<string> UploadAlbumArtAsync(Guid songId, IBrowserFile file)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            content.Add(fileContent, "file", file.Name);

            var response = await _httpClient.PostAsync($"api/songs/{songId}/upload-art", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync() ?? throw new Exception("Failed to upload album art");
        }

        public async Task<SongRating> RateSongAsync(Guid songId, SongRating rating)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/songs/{songId}/rate", rating);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SongRating>() ?? throw new Exception("Failed to rate song");
        }
    }
}
