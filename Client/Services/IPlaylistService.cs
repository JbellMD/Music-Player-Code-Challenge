using music_manager_starter.Shared;

namespace music_manager_starter.Client.Services
{
    public interface IPlaylistService
    {
        Task<IEnumerable<Playlist>> GetPlaylistsAsync();
        Task<Playlist> GetPlaylistAsync(int id);
        Task<Playlist> CreatePlaylistAsync(Playlist playlist);
        Task<Playlist> UpdatePlaylistAsync(Playlist playlist);
        Task DeletePlaylistAsync(int id);
        Task AddSongToPlaylistAsync(int playlistId, int songId);
        Task RemoveSongFromPlaylistAsync(int playlistId, int songId);
    }
}
