using music_manager_starter.Shared;

namespace music_manager_starter.Client.Services;

public interface ISongService
{
    Task<List<Song>> GetSongsAsync(string? searchTerm = null);
    Task<Song> AddSongAsync(Song song);
    Task<Song> UpdateSongAsync(Song song);
    Task DeleteSongAsync(Guid id);
}
