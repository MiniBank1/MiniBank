using System;
using System.Collections.Generic;

namespace Domain.Entity;

public partial class PaymentType
{
    public int PaymentTypeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
