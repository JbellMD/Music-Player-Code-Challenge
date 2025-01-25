using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Server.Data;
using music_manager_starter.Server.Hubs;
using music_manager_starter.Shared;

namespace music_manager_starter.Server.Controllers
{
    public class PlaylistsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PlaylistsController(
            ApplicationDbContext context,
            ILogger<PlaylistsController> logger,
            NotificationHub notificationHub)
            : base(logger, notificationHub)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Playlist>>> GetPlaylists()
        {
            try
            {
                var playlists = await _context.Playlists
                    .Include(p => p.Songs)
                        .ThenInclude(ps => ps.Song)
                    .ToListAsync();

                return Ok(playlists);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "retrieving playlists");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Playlist>> GetPlaylist(Guid id)
        {
            try
            {
                var playlist = await _context.Playlists
                    .Include(p => p.Songs)
                        .ThenInclude(ps => ps.Song)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (playlist == null)
                {
                    return NotFound();
                }

                return Ok(playlist);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "retrieving playlist");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Playlist>> CreatePlaylist([FromBody] Playlist playlist)
        {
            try
            {
                playlist.Id = Guid.NewGuid();
                playlist.CreatedAt = DateTime.UtcNow;
                playlist.UpdatedAt = DateTime.UtcNow;

                _context.Playlists.Add(playlist);
                await _context.SaveChangesAsync();

                await _notificationHub.SendNewPlaylistNotification(playlist);

                return CreatedAtAction(nameof(GetPlaylist), new { id = playlist.Id }, playlist);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "creating playlist");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlaylist(Guid id, [FromBody] Playlist playlist)
        {
            try
            {
                if (id != playlist.Id)
                {
                    return BadRequest();
                }

                playlist.UpdatedAt = DateTime.UtcNow;
                _context.Entry(playlist).State = EntityState.Modified;
                _context.Entry(playlist).Property(x => x.CreatedAt).IsModified = false;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex, "updating playlist");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlaylist(Guid id)
        {
            try
            {
                var playlist = await _context.Playlists.FindAsync(id);
                if (playlist == null)
                {
                    return NotFound();
                }

                _context.Playlists.Remove(playlist);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex, "deleting playlist");
            }
        }

        [HttpPost("{id}/songs")]
        public async Task<IActionResult> AddSongToPlaylist(Guid id, [FromBody] Guid songId)
        {
            try
            {
                var playlist = await _context.Playlists
                    .Include(p => p.Songs)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (playlist == null)
                {
                    return NotFound("Playlist not found");
                }

                var song = await _context.Songs.FindAsync(songId);
                if (song == null)
                {
                    return NotFound("Song not found");
                }

                if (playlist.Songs.Any(ps => ps.SongId == songId))
                {
                    return BadRequest("Song already exists in playlist");
                }

                var playlistSong = new PlaylistSong
                {
                    PlaylistId = id,
                    SongId = songId,
                    Order = playlist.Songs.Count + 1,
                    AddedAt = DateTime.UtcNow
                };

                playlist.Songs.Add(playlistSong);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return HandleException(ex, "adding song to playlist");
            }
        }

        [HttpDelete("{id}/songs/{songId}")]
        public async Task<IActionResult> RemoveSongFromPlaylist(Guid id, Guid songId)
        {
            try
            {
                var playlistSong = await _context.PlaylistSongs
                    .FirstOrDefaultAsync(ps => ps.PlaylistId == id && ps.SongId == songId);

                if (playlistSong == null)
                {
                    return NotFound();
                }

                _context.PlaylistSongs.Remove(playlistSong);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex, "removing song from playlist");
            }
        }

        [HttpPut("{id}/songs/reorder")]
        public async Task<IActionResult> ReorderPlaylistSongs(Guid id, [FromBody] List<PlaylistSong> songs)
        {
            try
            {
                var playlist = await _context.Playlists
                    .Include(p => p.Songs)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (playlist == null)
                {
                    return NotFound();
                }

                foreach (var song in songs)
                {
                    var existingSong = playlist.Songs.FirstOrDefault(ps => ps.SongId == song.SongId);
                    if (existingSong != null)
                    {
                        existingSong.Order = song.Order;
                    }
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex, "reordering playlist songs");
            }
        }
    }
}
