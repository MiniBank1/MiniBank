using System;
using System.Collections.Generic;

namespace Domain.Entity;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int? SenderAccountId { get; set; }

    public int? ReceiverAccountId { get; set; }

    public decimal Amount { get; set; }

    public string TransactionType { get; set; } = null!;

    public string? Description { get; set; }

    public int? PaymentTypeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual PaymentType? PaymentType { get; set; }

    public virtual Account? ReceiverAccount { get; set; }

    public virtual Account? SenderAccount { get; set; }
}
