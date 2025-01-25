using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.InMemory;
using Moq;
using music_manager_starter.Server.Controllers;
using music_manager_starter.Server.Data;
using music_manager_starter.Server.Hubs;
using music_manager_starter.Shared;
using Xunit;

namespace music_manager_starter.Server.Tests
{
    public class PlaylistsControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<PlaylistsController>> _loggerMock;
        private readonly Mock<NotificationHub> _notificationHubMock;
        private readonly PlaylistsController _controller;

        public PlaylistsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<PlaylistsController>>();
            _notificationHubMock = new Mock<NotificationHub>();
            _controller = new PlaylistsController(_context, _loggerMock.Object, _notificationHubMock.Object);
        }

        [Fact]
        public async Task CreatePlaylist_ValidPlaylist_ReturnsOk()
        {
            // Arrange
            var playlist = new Playlist
            {
                Name = "Test Playlist",
                Description = "Test Description"
            };

            // Act
            var result = await _controller.CreatePlaylist(playlist);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Playlist>(actionResult.Value);
            Assert.Equal(playlist.Name, returnValue.Name);
        }

        [Fact]
        public async Task GetPlaylist_ValidId_ReturnsPlaylist()
        {
            // Arrange
            var playlist = new Playlist
            {
                Name = "Test Playlist",
                Description = "Test Description"
            };
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetPlaylist(playlist.Id);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Playlist>(actionResult.Value);
            Assert.Equal(playlist.Name, returnValue.Name);
        }

        [Fact]
        public async Task GetPlaylist_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetPlaylist(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
