namespace Application.DTOs;

public class RecentNotificationDto
{
    public int NotificationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? CreatedAt { get; set; }
}