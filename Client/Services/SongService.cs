using Microsoft.AspNetCore.Components.Forms;
using music_manager_starter.Shared;
using System.Net.Http.Json;

namespace music_manager_starter.Client.Services
{
    public class SongService : ISongService
    {
        private readonly HttpService _httpService;

        public SongService(HttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<IEnumerable<Song>> GetSongsAsync(string? searchTerm = null)
        {
            var query = string.IsNullOrEmpty(searchTerm) ? "" : $"?search={searchTerm}";
            return await _httpService.GetAsync<IEnumerable<Song>>($"api/songs{query}") ?? Array.Empty<Song>();
        }

        public async Task<Song> GetSongAsync(int id)
        {
            return await _httpService.GetAsync<Song>($"api/songs/{id}") ?? throw new Exception("Song not found");
        }

        public async Task<Song> CreateSongAsync(Song song, Stream fileStream, string fileName)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fileStream), "file", fileName);
            content.Add(new StringContent(song.Title), "title");
            content.Add(new StringContent(song.Artist), "artist");
            content.Add(new StringContent(song.Album), "album");
            content.Add(new StringContent(song.Genre), "genre");
            
            return await _httpService.PostAsync<Song>("api/songs", content) ?? throw new Exception("Failed to create song");
        }

        public async Task<Song> UpdateSongAsync(Song song)
        {
            return await _httpService.PutAsync<Song>($"api/songs/{song.Id}", song) ?? throw new Exception("Failed to update song");
        }

        public async Task DeleteSongAsync(int id)
        {
            await _httpService.DeleteAsync($"api/songs/{id}");
        }

        public async Task<SongRating> RateSongAsync(int songId, int rating)
        {
            var ratingObj = new SongRating
            {
                SongId = songId,
                Rating = rating,
                RatedAt = DateTime.UtcNow
            };

            return await _httpService.PostAsync<SongRating>($"api/songs/{songId}/rate", ratingObj) ?? 
                   throw new Exception("Failed to rate song");
        }
    }
}
