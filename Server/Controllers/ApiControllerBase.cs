using Microsoft.AspNetCore.Mvc;
using music_manager_starter.Server.Hubs;

namespace music_manager_starter.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected readonly ILogger _logger;
        protected readonly NotificationHub _notificationHub;

        protected ApiControllerBase(ILogger logger, NotificationHub notificationHub)
        {
            _logger = logger;
            _notificationHub = notificationHub;
        }

        protected IActionResult HandleException(Exception ex, string operation)
        {
            _logger.LogError(ex, "Error during {Operation}: {Message}", operation, ex.Message);
            return StatusCode(500, $"An error occurred during {operation}. Please try again later.");
        }
    }
}
