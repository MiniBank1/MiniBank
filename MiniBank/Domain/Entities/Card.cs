using System;
using System.Collections.Generic;

namespace Domain.Entity;

public partial class Card
{
    public int CardId { get; set; }

    public int AccountId { get; set; }

    public string? CardNumber { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string? Cvv { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
}
