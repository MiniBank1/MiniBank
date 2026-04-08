namespace Application.DTOs;

public class AccountSummaryDto
{
    public int AccountId { get; set; }
    public string Iban { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime? CreatedAt { get; set; }

    public string FullName { get; set; } = string.Empty;
}
