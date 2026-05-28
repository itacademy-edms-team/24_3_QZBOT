using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebTests.Services.Interfaces;

namespace WebTests.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notifications = await _notificationService.GetUserNotificationAsync(userId);

            return Ok(notifications);
        }

        [Authorize]
        [HttpPost("read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _notificationService.MarkAsReadAsync(id, userId);

            return Ok();
        }
    }
}
