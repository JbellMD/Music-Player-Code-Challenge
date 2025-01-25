using Microsoft.AspNetCore.Components.Forms;
using music_manager_starter.Shared;
using System.IO;

namespace music_manager_starter.Client.Services;

public interface ISongService
{
    Task<IEnumerable<Song>> GetSongsAsync(string? searchTerm = null);
    Task<Song> GetSongAsync(int id);
    Task<Song> CreateSongAsync(Song song, Stream fileStream, string fileName);
    Task<Song> UpdateSongAsync(Song song);
    Task DeleteSongAsync(int id);
    Task<SongRating> RateSongAsync(int songId, int rating);
}
