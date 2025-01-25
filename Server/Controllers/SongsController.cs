using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Server.Data;
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
        public async Task<ActionResult<IEnumerable<Song>>> GetSongs()
        {
            var songs = await _context.Songs
                .Include(s => s.Ratings)
                .ToListAsync();

            return Ok(songs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Song>> GetSong(int id)
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

        [HttpPost]
        public async Task<ActionResult<Song>> CreateSong([FromForm] Song song, IFormFile file)
        {
            if (file != null)
            {
                using var stream = file.OpenReadStream();
                var fileName = await _fileUploadService.UploadFileAsync(stream, file.FileName);
                song.FilePath = fileName;
            }

            song.Id = Guid.NewGuid();
            song.CreatedAt = DateTime.UtcNow;
            song.UpdatedAt = DateTime.UtcNow;

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            await _notificationHub.SendNewSongNotification(song);

            return Ok(song);
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
        public async Task<ActionResult<SongRating>> RateSong(int id, [FromBody] int rating)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            var songRating = new SongRating
            {
                SongId = id,
                Rating = rating,
                RatedAt = DateTime.UtcNow
            };

            _context.SongRatings.Add(songRating);
            await _context.SaveChangesAsync();

            return Ok(songRating);
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
