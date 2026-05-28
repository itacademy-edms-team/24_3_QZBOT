using WebTests.Models;

namespace WebTests.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateAsync(
            string userId,
            string title,
            string message,
            string? link = null);

        Task<List<Notification>> GetUserNotificationAsync(
            string userId);

        Task MarkAsReadAsync(
            int notificationId,
            string userId);
    }
}
