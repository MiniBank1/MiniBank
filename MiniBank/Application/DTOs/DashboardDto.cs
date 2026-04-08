using Application.DTOs;

namespace Application.DTOs;

public class DashboardDto
{
    public AccountSummaryDto? DemandAccount { get; set; }
    public AccountSummaryDto? TimeDepositAccount { get; set; }
    public List<RecentTransactionDto> RecentTransactions { get; set; } = new();
    public bool HasTimeDepositAccount { get; set; }
    public List<PaymentTypeDto> PaymentTypes { get; set; } = new();

    public DateTime? LastInterestDate { get; set; }
    public DateTime? NextInterestDate { get; set; }
    public decimal? TimeDepositInterestRate { get; set; }
}