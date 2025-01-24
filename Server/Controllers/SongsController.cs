using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Server.Hubs;
using music_manager_starter.Server.Services;
using music_manager_starter.Shared;

namespace music_manager_starter.Server.Controllers
{
    public class SongsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;

        public SongsController(
            ApplicationDbContext context,
            IFileUploadService fileUploadService,
            ILogger<SongsController> logger,
            NotificationHub notificationHub)
            : base(logger, notificationHub)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Song>>> GetSongs([FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Songs
                    .Include(s => s.Ratings)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(s =>
                        s.Title.ToLower().Contains(search) ||
                        s.Artist.ToLower().Contains(search) ||
                        s.Album.ToLower().Contains(search) ||
                        s.Genre.ToLower().Contains(search));
                }

                var songs = await query.ToListAsync();
                return Ok(songs);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "retrieving songs");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Song>> GetSong(Guid id)
        {
            try
            {
                var song = await _context.Songs
                    .Include(s => s.Ratings)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (song == null)
                {
                    return NotFound();
                }

                return Ok(song);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "retrieving song");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Song>> CreateSong([FromBody] Song song)
        {
            try
            {
                song.Id = Guid.NewGuid();
                song.CreatedAt = DateTime.UtcNow;
                song.UpdatedAt = DateTime.UtcNow;

                _context.Songs.Add(song);
                await _context.SaveChangesAsync();

                await _notificationHub.SendNewSongNotification(song);

                return CreatedAtAction(nameof(GetSong), new { id = song.Id }, song);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "creating song");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSong(Guid id, [FromBody] Song song)
        {
            try
            {
                if (id != song.Id)
                {
                    return BadRequest();
                }

                song.UpdatedAt = DateTime.UtcNow;
                _context.Entry(song).State = EntityState.Modified;
                _context.Entry(song).Property(x => x.CreatedAt).IsModified = false;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex, "updating song");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSong(Guid id)
        {
            try
            {
                var song = await _context.Songs.FindAsync(id);
                if (song == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(song.AlbumArtUrl))
                {
                    _fileUploadService.DeleteAlbumArt(song.AlbumArtUrl);
                }

                _context.Songs.Remove(song);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex, "deleting song");
            }
        }

        [HttpPost("{id}/rate")]
        public async Task<ActionResult<SongRating>> RateSong(Guid id, [FromBody] SongRating rating)
        {
            try
            {
                var song = await _context.Songs.FindAsync(id);
                if (song == null)
                {
                    return NotFound();
                }

                rating.Id = Guid.NewGuid();
                rating.SongId = id;
                rating.RatedAt = DateTime.UtcNow;

                _context.SongRatings.Add(rating);
                await _context.SaveChangesAsync();

                return Ok(rating);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "rating song");
            }
        }

        [HttpPost("{id}/upload-art")]
        public async Task<IActionResult> UploadAlbumArt(Guid id, [FromForm] IFormFile file)
        {
            try
            {
                var song = await _context.Songs.FindAsync(id);
                if (song == null)
                {
                    return NotFound();
                }

                // Delete existing album art if it exists
                if (!string.IsNullOrEmpty(song.AlbumArtUrl))
                {
                    _fileUploadService.DeleteAlbumArt(song.AlbumArtUrl);
                }

                // Save new album art
                var fileName = await _fileUploadService.SaveAlbumArtAsync(file);
                song.AlbumArtUrl = fileName;
                song.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { fileName });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "uploading album art");
            }
        }
    }
}
