using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.InMemory;
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
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<SongsController>> _loggerMock;
        private readonly Mock<IFileUploadService> _fileUploadServiceMock;
        private readonly Mock<NotificationHub> _notificationHubMock;
        private readonly SongsController _controller;

        public SongsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<SongsController>>();
            _fileUploadServiceMock = new Mock<IFileUploadService>();
            _notificationHubMock = new Mock<NotificationHub>();
            _controller = new SongsController(_context, _fileUploadServiceMock.Object, _loggerMock.Object, _notificationHubMock.Object);
        }

        [Fact]
        public async Task CreateSong_ValidSong_ReturnsCreatedAtAction()
        {
            // Arrange
            var song = new Song
            {
                Title = "Test Song",
                Artist = "Test Artist",
                Album = "Test Album",
                Genre = "Test Genre",
                ReleaseDate = DateTime.Now
            };

            // Act
            var result = await _controller.CreateSong(song, null);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Song>(actionResult.Value);
            Assert.Equal(song.Title, returnValue.Title);
        }

        [Fact]
        public async Task RateSong_ValidRating_ReturnsOk()
        {
            // Arrange
            var song = new Song
            {
                Title = "Test Song",
                Artist = "Test Artist"
            };
            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.RateSong(song.Id, 5);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<SongRating>(actionResult.Value);
            Assert.Equal(5, returnValue.Rating);
        }

        [Fact]
        public async Task RateSong_InvalidSong_ReturnsNotFound()
        {
            // Act
            var result = await _controller.RateSong(Guid.NewGuid(), 5);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
