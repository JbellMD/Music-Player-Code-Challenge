using music_manager_starter.Shared;
using Microsoft.AspNetCore.Components.Forms;

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
        private readonly IHttpService _httpService;
        private const string BaseUrl = "api/songs";

        public SongService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<List<Song>> GetSongsAsync(string? search = null)
        {
            var url = string.IsNullOrEmpty(search) ? BaseUrl : $"{BaseUrl}?search={Uri.EscapeDataString(search)}";
            return await _httpService.GetAsync<List<Song>>(url) ?? new List<Song>();
        }

        public async Task<Song> GetSongAsync(Guid id)
        {
            return await _httpService.GetAsync<Song>($"{BaseUrl}/{id}") 
                ?? throw new Exception("Song not found");
        }

        public async Task<Song> CreateSongAsync(Song song)
        {
            return await _httpService.PostAsync<Song>(BaseUrl, song) 
                ?? throw new Exception("Failed to create song");
        }

        public async Task UpdateSongAsync(Song song)
        {
            await _httpService.PutAsync<Song>($"{BaseUrl}/{song.Id}", song);
        }

        public async Task DeleteSongAsync(Guid id)
        {
            await _httpService.DeleteAsync($"{BaseUrl}/{id}");
        }

        public async Task<SongRating> RateSongAsync(Guid songId, SongRating rating)
        {
            return await _httpService.PostAsync<SongRating>($"{BaseUrl}/{songId}/rate", rating) 
                ?? throw new Exception("Failed to rate song");
        }

        public async Task<string> UploadAlbumArtAsync(Guid songId, IBrowserFile file)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            content.Add(fileContent, "file", file.Name);

            var response = await _httpService.PostAsync<dynamic>($"{BaseUrl}/{songId}/upload-art", content);
            return response?.fileName ?? throw new Exception("Failed to upload album art");
        }
    }
}
