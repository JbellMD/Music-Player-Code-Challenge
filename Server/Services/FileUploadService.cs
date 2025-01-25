using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Forms;

namespace music_manager_starter.Server.Services
{
    public interface IFileUploadService
    {
        Task<string> SaveAlbumArtAsync(IFormFile file);
        void DeleteAlbumArt(string fileName);
        Task<string> UploadFileAsync(Stream stream, string fileName);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveAlbumArtAsync(IFormFile file)
        {
            try
            {
                if (file == null)
                    throw new ArgumentNullException(nameof(file));

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                    throw new InvalidOperationException($"File type {extension} is not allowed.");

                if (file.Length > MaxFileSize)
                    throw new InvalidOperationException($"File size exceeds maximum limit of {MaxFileSize / 1024 / 1024}MB.");

                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "albumart");
                Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using var stream = file.OpenReadStream();
                using var fileStream = File.Create(filePath);
                await stream.CopyToAsync(fileStream);

                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading album art");
                throw;
            }
        }

        public void DeleteAlbumArt(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "albumart", fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting album art: {FileName}", fileName);
            }
        }

        public async Task<string> UploadFileAsync(Stream stream, string fileName)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await stream.CopyToAsync(fileStream);

            return uniqueFileName;
        }
    }
}
