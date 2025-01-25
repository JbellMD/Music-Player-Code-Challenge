using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly Mock<ILogger<PlaylistsController>> _loggerMock;
        private readonly Mock<NotificationHub> _hubMock;
        private readonly ApplicationDbContext _context;
        private readonly PlaylistsController _controller;

        public PlaylistsControllerTests()
        {
            _loggerMock = new Mock<ILogger<PlaylistsController>>();
            _hubMock = new Mock<NotificationHub>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new PlaylistsController(_context, _loggerMock.Object, _hubMock.Object);
        }

        [Fact]
        public async Task GetPlaylists_ReturnsAllPlaylists()
        {
            // Arrange
            var playlists = new List<Playlist>
            {
                new Playlist { Id = Guid.NewGuid(), Name = "Playlist 1" },
                new Playlist { Id = Guid.NewGuid(), Name = "Playlist 2" }
            };

            _context.Playlists.AddRange(playlists);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetPlaylists();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPlaylists = Assert.IsAssignableFrom<IEnumerable<Playlist>>(okResult.Value);
            Assert.Equal(2, returnedPlaylists.Count());
        }

        [Fact]
        public async Task CreatePlaylist_ReturnsCreatedAtAction()
        {
            // Arrange
            var playlist = new Playlist
            {
                Name = "New Playlist",
                Description = "Test Description"
            };

            // Act
            var result = await _controller.CreatePlaylist(playlist);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedPlaylist = Assert.IsType<Playlist>(createdAtActionResult.Value);
            Assert.Equal(playlist.Name, returnedPlaylist.Name);
        }

        [Fact]
        public async Task AddSongToPlaylist_WithValidIds_ReturnsOk()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                Name = "Test Playlist"
            };

            var song = new Song
            {
                Id = Guid.NewGuid(),
                Title = "Test Song"
            };

            _context.Playlists.Add(playlist);
            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.AddSongToPlaylist(playlist.Id, song.Id);

            // Assert
            Assert.IsType<OkResult>(result);
            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlist.Id && ps.SongId == song.Id);
            Assert.NotNull(playlistSong);
        }

        [Fact]
        public async Task RemoveSongFromPlaylist_WithValidIds_ReturnsNoContent()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                Name = "Test Playlist"
            };

            var song = new Song
            {
                Id = Guid.NewGuid(),
                Title = "Test Song"
            };

            var playlistSong = new PlaylistSong
            {
                PlaylistId = playlist.Id,
                SongId = song.Id,
                Order = 1
            };

            _context.Playlists.Add(playlist);
            _context.Songs.Add(song);
            _context.PlaylistSongs.Add(playlistSong);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.RemoveSongFromPlaylist(playlist.Id, song.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlist.Id && ps.SongId == song.Id));
        }
    }
}
