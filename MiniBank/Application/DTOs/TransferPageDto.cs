namespace Application.DTOs;

public class TransferPageDto
{
    public AccountSummaryDto? DemandAccount { get; set; }
    public AccountSummaryDto? TimeDepositAccount { get; set; }
    public List<PaymentTypeDto> PaymentTypes { get; set; } = new();
}