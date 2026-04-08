using System;
using System.Collections.Generic;

namespace Domain.Entity;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public int? TransactionId { get; set; }

    public string? Message { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Transaction? Transaction { get; set; }

    public virtual User User { get; set; } = null!;
}
