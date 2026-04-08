using System;
using System.Collections.Generic;

namespace Domain.Entity;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public string? UserSurname { get; set; }

    public string? TcNo { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
