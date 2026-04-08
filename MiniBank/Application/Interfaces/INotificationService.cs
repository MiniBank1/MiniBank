using Application.DTOs;

namespace Application.Interfaces;

public interface INotificationService
{
    Task<List<RecentNotificationDto>> GetUnreadNotificationsAsync(int userId, int count = 10);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task CreateAsync(int userId, string message, int? transactionId = null);
    Task<int> GetUnreadNotificationCountAsync(int userId);
}