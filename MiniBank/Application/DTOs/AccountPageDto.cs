namespace Application.DTOs
{
    public class AccountPageDto
    {
        public AccountSummaryDto? DemandAccount { get; set; }
        public AccountSummaryDto? TimeDepositAccount { get; set; }

        public CardDto? CurrentCard { get; set; }
        public CardDto? TimeDepositCard { get; set; }
        public bool HasTimeDepositAccount { get; set; }

        public bool HasCurrentCard => CurrentCard != null;
        public bool HasTimeDepositCard => TimeDepositCard != null;
        public bool HasAnyActiveCard => CurrentCard != null || TimeDepositCard != null;
    }
}
