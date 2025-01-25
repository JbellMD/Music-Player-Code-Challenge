using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Shared;

namespace music_manager_starter.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SongsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Song>>> GetSongs()
        {
            var songs = await _context.Songs
                .Include(s => s.PlaylistSongs)
                .ToListAsync();

            return Ok(songs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Song>> GetSong(int id)
        {
            var song = await _context.Songs
                .Include(s => s.PlaylistSongs)
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
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            song.FilePath = $"/uploads/{fileName}";
            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSong), new { id = song.Id }, song);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Song>> UpdateSong(int id, Song song)
        {
            if (id != song.Id)
            {
                return BadRequest();
            }

            _context.Entry(song).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SongExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return Ok(song);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSong(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(song.FilePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", song.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/rate")]
        public async Task<ActionResult<SongRating>> RateSong(int id, [FromBody] SongRating rating)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            rating.SongId = id;
            rating.RatedAt = DateTime.UtcNow;

            _context.SongRatings.Add(rating);
            await _context.SaveChangesAsync();

            // Update the song's average rating
            var averageRating = await _context.SongRatings
                .Where(r => r.SongId == id)
                .AverageAsync(r => r.Rating);

            song.Rating = (int)Math.Round(averageRating);
            await _context.SaveChangesAsync();

            return Ok(rating);
        }

        private bool SongExists(int id)
        {
            return _context.Songs.Any(e => e.Id == id);
        }
    }
}
