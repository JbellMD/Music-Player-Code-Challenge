using Microsoft.AspNetCore.Components.Forms;
using music_manager_starter.Shared;

namespace music_manager_starter.Client.Services;

public interface ISongService
{
    Task<List<Song>> GetSongsAsync(string? searchTerm = null);
    Task<Song> GetSongAsync(Guid id);
    Task<Song> AddSongAsync(Song song);
    Task<Song> UpdateSongAsync(Song song);
    Task DeleteSongAsync(Guid id);
    Task<string> UploadAlbumArtAsync(Guid songId, IBrowserFile file);
    Task<SongRating> RateSongAsync(Guid songId, SongRating rating);
}
