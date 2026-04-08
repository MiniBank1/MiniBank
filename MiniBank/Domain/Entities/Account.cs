using System;
using System.Collections.Generic;

namespace Domain.Entity;

public partial class Account
{
    public int AccountId { get; set; }

    public int UserId { get; set; }

    public string? Iban { get; set; }

    public string? AccountType { get; set; }

    public decimal? InterestRate { get; set; }

    public DateTime? LastInterestDate { get; set; }

    public decimal? Balance { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();

    public virtual ICollection<Transaction> TransactionReceiverAccounts { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionSenderAccounts { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;
}
