namespace Application.DTOs;

public class RecentTransactionDto
{
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public string CounterpartyFullName { get; set; } = string.Empty;
    public bool IsIncoming { get; set; }
}