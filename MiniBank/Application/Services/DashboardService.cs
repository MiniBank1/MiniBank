using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class DashboardService : IDashboardService
{
    private readonly MiniBankDbContext _context;
    private const decimal DefaultTimeDepositInterestRate = 1m; // %1 sabit faiz

    public DashboardService(MiniBankDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardAsync(int userId)
    {
        var accounts = await _context.Accounts
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        var demandAccount = accounts.FirstOrDefault(x => x.AccountType == "CURRENT");
        var timeDepositAccount = accounts.FirstOrDefault(x => x.AccountType == "TIME DEPOSIT");

        var recentTransactionsRaw = await _context.Transactions
            .Include(x => x.SenderAccount)
                .ThenInclude(x => x.User)
            .Include(x => x.ReceiverAccount)
                .ThenInclude(x => x.User)
            .Where(x =>
                (x.SenderAccount != null && x.SenderAccount.UserId == userId) ||
                (x.ReceiverAccount != null && x.ReceiverAccount.UserId == userId))
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentTransactions = recentTransactionsRaw
            .Select(x =>
            {
                var isIncoming = x.ReceiverAccount != null && x.ReceiverAccount.UserId == userId;

                var counterpartyName = x.TransactionType == "interest"
                    ? "Bank Interest"
                    : isIncoming
                        ? $"{x.SenderAccount?.User?.UserName} {x.SenderAccount?.User?.UserSurname}".Trim()
                        : $"{x.ReceiverAccount?.User?.UserName} {x.ReceiverAccount?.User?.UserSurname}".Trim();

                return new RecentTransactionDto
                {
                    Amount = x.Amount,
                    TransactionType = x.TransactionType ?? string.Empty,
                    Description = x.Description ?? string.Empty,
                    CreatedAt = x.CreatedAt,
                    CounterpartyFullName = counterpartyName,
                    IsIncoming = isIncoming
                };
            })
            .ToList();

        var paymentTypes = await _context.PaymentTypes
            .OrderBy(x => x.Name)
            .Select(x => new PaymentTypeDto
            {
                PaymentTypeId = x.PaymentTypeId,
                Name = x.Name ?? string.Empty
            })
            .ToListAsync();

        return new DashboardDto
        {
            DemandAccount = demandAccount == null ? null : new AccountSummaryDto
            {
                AccountId = demandAccount.AccountId,
                Iban = demandAccount.Iban ?? string.Empty,
                AccountType = demandAccount.AccountType ?? string.Empty,
                Balance = demandAccount.Balance ?? 0,
                CreatedAt = demandAccount.CreatedAt,
                FullName = $"{demandAccount.User?.UserName} {demandAccount.User?.UserSurname}".Trim()
            },

            TimeDepositAccount = timeDepositAccount == null ? null : new AccountSummaryDto
            {
                AccountId = timeDepositAccount.AccountId,
                Iban = timeDepositAccount.Iban ?? string.Empty,
                AccountType = timeDepositAccount.AccountType ?? string.Empty,
                Balance = timeDepositAccount.Balance ?? 0,
                CreatedAt = timeDepositAccount.CreatedAt,
                FullName = $"{timeDepositAccount.User?.UserName} {timeDepositAccount.User?.UserSurname}".Trim()
            },

            RecentTransactions = recentTransactions,
            HasTimeDepositAccount = timeDepositAccount != null,
            PaymentTypes = paymentTypes,

            LastInterestDate = timeDepositAccount?.LastInterestDate,
            NextInterestDate = timeDepositAccount?.LastInterestDate?.AddHours(24),
            TimeDepositInterestRate = timeDepositAccount != null ? DefaultTimeDepositInterestRate : null
        };
    }
}