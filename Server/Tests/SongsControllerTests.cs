using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using music_manager_starter.Server.Controllers;
using music_manager_starter.Server.Data;
using music_manager_starter.Server.Hubs;
using music_manager_starter.Server.Services;
using music_manager_starter.Shared;
using Xunit;

namespace music_manager_starter.Server.Tests
{
    public class SongsControllerTests
    {
        private readonly Mock<ILogger<SongsController>> _loggerMock;
        private readonly Mock<NotificationHub> _hubMock;
        private readonly Mock<IFileUploadService> _fileUploadServiceMock;
        private readonly ApplicationDbContext _context;
        private readonly SongsController _controller;

        public SongsControllerTests()
        {
            _loggerMock = new Mock<ILogger<SongsController>>();
            _hubMock = new Mock<NotificationHub>();
            _fileUploadServiceMock = new Mock<IFileUploadService>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new SongsController(_context, _fileUploadServiceMock.Object, _loggerMock.Object, _hubMock.Object);
        }

        [Fact]
        public async Task GetSongs_ReturnsAllSongs()
        {
            // Arrange
            var songs = new List<Song>
            {
                new Song { Id = Guid.NewGuid(), Title = "Song 1", Artist = "Artist 1" },
                new Song { Id = Guid.NewGuid(), Title = "Song 2", Artist = "Artist 2" }
            };

            _context.Songs.AddRange(songs);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetSongs();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSongs = Assert.IsAssignableFrom<IEnumerable<Song>>(okResult.Value);
            Assert.Equal(2, returnedSongs.Count());
        }

        [Fact]
        public async Task CreateSong_ReturnsCreatedAtAction()
        {
            // Arrange
            var song = new Song
            {
                Title = "New Song",
                Artist = "New Artist",
                Album = "New Album"
            };

            // Act
            var result = await _controller.CreateSong(song);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedSong = Assert.IsType<Song>(createdAtActionResult.Value);
            Assert.Equal(song.Title, returnedSong.Title);
            Assert.Equal(song.Artist, returnedSong.Artist);
        }

        [Fact]
        public async Task GetSong_WithValidId_ReturnsSong()
        {
            // Arrange
            var song = new Song
            {
                Id = Guid.NewGuid(),
                Title = "Test Song",
                Artist = "Test Artist"
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetSong(song.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSong = Assert.IsType<Song>(okResult.Value);
            Assert.Equal(song.Id, returnedSong.Id);
        }

        [Fact]
        public async Task GetSong_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetSong(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteSong_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var song = new Song
            {
                Id = Guid.NewGuid(),
                Title = "Test Song",
                Artist = "Test Artist"
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteSong(song.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(await _context.Songs.FindAsync(song.Id));
        }
    }
}
