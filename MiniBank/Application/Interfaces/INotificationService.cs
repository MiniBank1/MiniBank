using Application.DTOs;

namespace Application.Interfaces;

public interface INotificationService
{
    Task<List<RecentNotificationDto>> GetRecentNotificationsAsync(int userId, int count = 5);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task CreateAsync(int userId, string message, int? transactionId = null);
}