using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Server.Controllers;
using music_manager_starter.Shared;
using Xunit;

namespace music_manager_starter.Server.Tests
{
    public class PlaylistsControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly PlaylistsController _controller;

        public PlaylistsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new PlaylistsController(_context);

            // Clear the database before each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task GetPlaylists_ReturnsAllPlaylists()
        {
            // Arrange
            var playlist1 = new Playlist { Name = "Test Playlist 1" };
            var playlist2 = new Playlist { Name = "Test Playlist 2" };
            _context.Playlists.AddRange(playlist1, playlist2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetPlaylists();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var playlists = Assert.IsAssignableFrom<IEnumerable<Playlist>>(actionResult.Value);
            Assert.Equal(2, playlists.Count());
        }

        [Fact]
        public async Task GetPlaylist_ReturnsPlaylist_WhenPlaylistExists()
        {
            // Arrange
            var playlist = new Playlist { Name = "Test Playlist" };
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetPlaylist(playlist.Id);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPlaylist = Assert.IsType<Playlist>(actionResult.Value);
            Assert.Equal(playlist.Id, returnedPlaylist.Id);
            Assert.Equal(playlist.Name, returnedPlaylist.Name);
        }

        [Fact]
        public async Task GetPlaylist_ReturnsNotFound_WhenPlaylistDoesNotExist()
        {
            // Act
            var result = await _controller.GetPlaylist(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreatePlaylist_CreatesNewPlaylist()
        {
            // Arrange
            var playlist = new Playlist { Name = "New Playlist" };

            // Act
            var result = await _controller.CreatePlaylist(playlist);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdPlaylist = Assert.IsType<Playlist>(actionResult.Value);
            Assert.Equal(playlist.Name, createdPlaylist.Name);
            Assert.NotEqual(0, createdPlaylist.Id);

            // Verify it was added to the database
            var dbPlaylist = await _context.Playlists.FindAsync(createdPlaylist.Id);
            Assert.NotNull(dbPlaylist);
            Assert.Equal(playlist.Name, dbPlaylist.Name);
        }

        [Fact]
        public async Task UpdatePlaylist_UpdatesExistingPlaylist()
        {
            // Arrange
            var playlist = new Playlist { Name = "Original Name" };
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            playlist.Name = "Updated Name";

            // Act
            var result = await _controller.UpdatePlaylist(playlist.Id, playlist);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var updatedPlaylist = Assert.IsType<Playlist>(actionResult.Value);
            Assert.Equal("Updated Name", updatedPlaylist.Name);

            // Verify it was updated in the database
            var dbPlaylist = await _context.Playlists.FindAsync(playlist.Id);
            Assert.NotNull(dbPlaylist);
            Assert.Equal("Updated Name", dbPlaylist.Name);
        }

        [Fact]
        public async Task DeletePlaylist_DeletesPlaylist()
        {
            // Arrange
            var playlist = new Playlist { Name = "Test Playlist" };
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeletePlaylist(playlist.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify it was deleted from the database
            var dbPlaylist = await _context.Playlists.FindAsync(playlist.Id);
            Assert.Null(dbPlaylist);
        }
    }
}
