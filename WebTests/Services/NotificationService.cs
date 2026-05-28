using Microsoft.EntityFrameworkCore;
using WebTests.Data;
using WebTests.Models;
using WebTests.Services.Interfaces;

namespace WebTests.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(string userId, string title, string message, string? link = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Link = link,
                CreatedDate = DateTime.UtcNow,
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotificationAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return;

            notification.IsRead = true;

            await _context.SaveChangesAsync();
        }
    }
}
