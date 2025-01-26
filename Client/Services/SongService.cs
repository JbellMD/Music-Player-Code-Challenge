using Microsoft.AspNetCore.Components.Forms;
using music_manager_starter.Shared;
using System.Net.Http.Json;

namespace music_manager_starter.Client.Services
{
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

        public async Task<Song> GetSongAsync(int id)
        {
            return await _httpService.GetAsync<Song>($"api/songs/{id}") ?? 
                   throw new Exception($"Song with id {id} not found");
        }

        public async Task<Song> CreateSongAsync(Song song, Stream fileStream, string fileName)
        {
            // First upload the file
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync("api/songs/upload", content);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to upload song file");

            var filePath = await response.Content.ReadAsStringAsync();
            song.FilePath = filePath;

            // Then create the song
            return await _httpService.PostAsync<Song>("api/songs", song) ?? 
                   throw new Exception("Failed to create song");
        }

        public async Task<Song> UpdateSongAsync(Song song)
        {
            return await _httpService.PutAsync<Song>($"api/songs/{song.Id}", song) ?? 
                   throw new Exception($"Failed to update song with id {song.Id}");
        }

        public async Task DeleteSongAsync(int id)
        {
            await _httpService.DeleteAsync($"api/songs/{id}");
        }

        public async Task<SongRating> RateSongAsync(int songId, int rating)
        {
            return await _httpService.PostAsync<SongRating>($"api/songs/{songId}/rate", new { Rating = rating }) ?? 
                   throw new Exception($"Failed to rate song with id {songId}");
        }
    }
}
