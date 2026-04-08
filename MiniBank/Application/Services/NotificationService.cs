using Application.DTOs;
using Application.Interfaces;
using Domain.Entity;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class NotificationService : INotificationService
{
    private readonly MiniBankDbContext _context;

    public NotificationService(MiniBankDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecentNotificationDto>> GetRecentNotificationsAsync(int userId, int count = 5)
    {
        return await _context.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .Select(x => new RecentNotificationDto
            {
                NotificationId = x.NotificationId,
                Message = x.Message ?? string.Empty,
                IsRead = x.IsRead ?? false,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.UserId == userId);

        if (notification == null)
            return;

        notification.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task CreateAsync(int userId, string message, int? transactionId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            TransactionId = transactionId,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }
}