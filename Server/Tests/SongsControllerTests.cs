using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Server.Controllers;
using music_manager_starter.Shared;
using Microsoft.AspNetCore.Http;
using System.Text;
using Xunit;

namespace music_manager_starter.Server.Tests
{
    public class SongsControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly SongsController _controller;

        public SongsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new SongsController(_context);

            // Clear the database before each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task GetSongs_ReturnsAllSongs()
        {
            // Arrange
            var song1 = new Song { Title = "Test Song 1", Artist = "Test Artist 1" };
            var song2 = new Song { Title = "Test Song 2", Artist = "Test Artist 2" };
            _context.Songs.AddRange(song1, song2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetSongs();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var songs = Assert.IsAssignableFrom<IEnumerable<Song>>(actionResult.Value);
            Assert.Equal(2, songs.Count());
        }

        [Fact]
        public async Task GetSong_ReturnsSong_WhenSongExists()
        {
            // Arrange
            var song = new Song { Title = "Test Song", Artist = "Test Artist" };
            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetSong(song.Id);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSong = Assert.IsType<Song>(actionResult.Value);
            Assert.Equal(song.Id, returnedSong.Id);
            Assert.Equal(song.Title, returnedSong.Title);
        }

        [Fact]
        public async Task GetSong_ReturnsNotFound_WhenSongDoesNotExist()
        {
            // Act
            var result = await _controller.GetSong(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateSong_CreatesNewSong()
        {
            // Arrange
            var song = new Song { Title = "New Song", Artist = "New Artist" };
            var fileContent = "Test file content";
            var fileName = "test.mp3";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            var file = new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "audio/mpeg"
            };

            // Act
            var result = await _controller.CreateSong(song, file);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdSong = Assert.IsType<Song>(actionResult.Value);
            Assert.Equal(song.Title, createdSong.Title);
            Assert.NotEqual(0, createdSong.Id);
            Assert.NotNull(createdSong.FilePath);

            // Verify it was added to the database
            var dbSong = await _context.Songs.FindAsync(createdSong.Id);
            Assert.NotNull(dbSong);
            Assert.Equal(song.Title, dbSong.Title);
        }

        [Fact]
        public async Task UpdateSong_UpdatesExistingSong()
        {
            // Arrange
            var song = new Song { Title = "Original Title", Artist = "Original Artist" };
            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            song.Title = "Updated Title";

            // Act
            var result = await _controller.UpdateSong(song.Id, song);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var updatedSong = Assert.IsType<Song>(actionResult.Value);
            Assert.Equal("Updated Title", updatedSong.Title);

            // Verify it was updated in the database
            var dbSong = await _context.Songs.FindAsync(song.Id);
            Assert.NotNull(dbSong);
            Assert.Equal("Updated Title", dbSong.Title);
        }

        [Fact]
        public async Task DeleteSong_DeletesSong()
        {
            // Arrange
            var song = new Song { Title = "Test Song", Artist = "Test Artist" };
            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteSong(song.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify it was deleted from the database
            var dbSong = await _context.Songs.FindAsync(song.Id);
            Assert.Null(dbSong);
        }

        [Fact]
        public async Task RateSong_AddsSongRating()
        {
            // Arrange
            var song = new Song { Title = "Test Song", Artist = "Test Artist" };
            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            var rating = new SongRating { Rating = 5 };

            // Act
            var result = await _controller.RateSong(song.Id, rating);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var addedRating = Assert.IsType<SongRating>(actionResult.Value);
            Assert.Equal(5, addedRating.Rating);
            Assert.Equal(song.Id, addedRating.SongId);

            // Verify the song's rating was updated
            var dbSong = await _context.Songs.FindAsync(song.Id);
            Assert.NotNull(dbSong);
            Assert.Equal(5, dbSong.Rating);
        }
    }
}
