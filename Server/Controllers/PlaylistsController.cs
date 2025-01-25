using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Shared;

namespace music_manager_starter.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PlaylistsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Playlist>>> GetPlaylists()
        {
            var playlists = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                .ToListAsync();

            return Ok(playlists);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Playlist>> GetPlaylist(int id)
        {
            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (playlist == null)
            {
                return NotFound();
            }

            return Ok(playlist);
        }

        [HttpPost]
        public async Task<ActionResult<Playlist>> CreatePlaylist(Playlist playlist)
        {
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlaylist), new { id = playlist.Id }, playlist);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Playlist>> UpdatePlaylist(int id, Playlist playlist)
        {
            if (id != playlist.Id)
            {
                return BadRequest();
            }

            _context.Entry(playlist).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlaylistExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return Ok(playlist);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlaylist(int id)
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

        [HttpPost("{playlistId}/songs/{songId}")]
        public async Task<ActionResult<Playlist>> AddSongToPlaylist(int playlistId, int songId)
        {
            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .FirstOrDefaultAsync(p => p.Id == playlistId);

            if (playlist == null)
            {
                return NotFound("Playlist not found");
            }

            var song = await _context.Songs.FindAsync(songId);
            if (song == null)
            {
                return NotFound("Song not found");
            }

            var playlistSong = new PlaylistSong
            {
                PlaylistId = playlistId,
                SongId = songId,
                Order = playlist.PlaylistSongs.Count
            };

            playlist.PlaylistSongs.Add(playlistSong);
            await _context.SaveChangesAsync();

            return Ok(playlist);
        }

        [HttpDelete("{playlistId}/songs/{songId}")]
        public async Task<ActionResult<Playlist>> RemoveSongFromPlaylist(int playlistId, int songId)
        {
            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .FirstOrDefaultAsync(p => p.Id == playlistId);

            if (playlist == null)
            {
                return NotFound("Playlist not found");
            }

            var playlistSong = playlist.PlaylistSongs
                .FirstOrDefault(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (playlistSong == null)
            {
                return NotFound("Song not found in playlist");
            }

            playlist.PlaylistSongs.Remove(playlistSong);

            // Reorder remaining songs
            var remainingSongs = playlist.PlaylistSongs.OrderBy(ps => ps.Order).ToList();
            for (int i = 0; i < remainingSongs.Count; i++)
            {
                remainingSongs[i].Order = i;
            }

            await _context.SaveChangesAsync();

            return Ok(playlist);
        }

        [HttpPut("{playlistId}/songs/reorder")]
        public async Task<ActionResult<Playlist>> ReorderPlaylistSongs(int playlistId, [FromBody] List<PlaylistSong> songs)
        {
            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .FirstOrDefaultAsync(p => p.Id == playlistId);

            if (playlist == null)
            {
                return NotFound("Playlist not found");
            }

            foreach (var song in songs)
            {
                var playlistSong = playlist.PlaylistSongs
                    .FirstOrDefault(ps => ps.PlaylistId == playlistId && ps.SongId == song.SongId);

                if (playlistSong != null)
                {
                    playlistSong.Order = song.Order;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(playlist);
        }

        private bool PlaylistExists(int id)
        {
            return _context.Playlists.Any(e => e.Id == id);
        }
    }
}
