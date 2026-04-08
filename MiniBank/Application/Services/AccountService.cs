using Application.DTOs;
using Application.Interfaces;
using Domain.Entity;
using Infrastructure.Data;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Application.Services;

public class AccountService : IAccountService
{
    private readonly MiniBankDbContext _context;
    private readonly ICardService _cardService;
    private readonly INotificationService _notificationService;

    private const decimal DefaultTimeDepositInterestRate = 1m; // %1 sabit faiz

    public AccountService(
        MiniBankDbContext context,
        ICardService cardService,
        INotificationService notificationService)
    {
        _context = context;
        _cardService = cardService;
        _notificationService = notificationService;
    }

    public async Task<(bool Success, string Message)> CreateTimeDepositAccountAsync(int userId)
    {
        var accounts = await _context.Accounts
            .Where(x => x.UserId == userId)
            .ToListAsync();

        var hasCurrentAccount = accounts.Any(x => x.AccountType == "CURRENT");
        if (!hasCurrentAccount)
            return (false, "First, you must have a current account.");

        var hasTimeDepositAccount = accounts.Any(x => x.AccountType == "TIME DEPOSIT");
        if (hasTimeDepositAccount)
            return (false, "You already have a time deposit account.");

        var account = new Account
        {
            UserId = userId,
            Iban = IbanGenerator.GenerateFakeIban(),
            AccountType = "TIME DEPOSIT",
            InterestRate = DefaultTimeDepositInterestRate,
            LastInterestDate = DateTime.Now,
            Balance = 0,
            CreatedAt = DateTime.Now
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var cardResult = await _cardService.CreateCardForAccountAsync(account.AccountId);

        if (!cardResult.Success)
            return (false, "Time deposit account created, but card could not be created.");

        await _notificationService.CreateAsync(
            userId,
            "Your time deposit account has been created successfully."
        );

        return (true, "Time deposit account and card created successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteTimeDepositAccountAsync(int userId)
    {
        var timeDepositAccount = await _context.Accounts
            .FirstOrDefaultAsync(x => x.UserId == userId && x.AccountType == "TIME DEPOSIT");

        if (timeDepositAccount == null)
            return (false, "No time deposit account found to be deleted.");

        var balance = timeDepositAccount.Balance ?? 0;

        if (balance > 0)
            return (false, "The time deposit account cannot be deleted because it has a balance. First, transfer the balance to your current account.");

        if (balance < 0)
            return (false, "The time deposit account cannot be deleted because the balance status is invalid.");

        var relatedTransactions = await _context.Transactions
            .Where(x => x.SenderAccountId == timeDepositAccount.AccountId ||
                        x.ReceiverAccountId == timeDepositAccount.AccountId)
            .ToListAsync();

        if (relatedTransactions.Any())
        {
            _context.Transactions.RemoveRange(relatedTransactions);
        }

        await _cardService.DeleteCardsByAccountIdAsync(timeDepositAccount.AccountId);

        _context.Accounts.Remove(timeDepositAccount);
        await _context.SaveChangesAsync();

        await _notificationService.CreateAsync(
            userId,
            "Your time deposit account has been deleted successfully."
        );

        return (true, "Time deposit account, related cards, and transaction history have been successfully deleted.");
    }

    public async Task<(bool Success, string Message)> TransferDemandToTimeDepositAsync(int userId, decimal amount)
    {
        if (amount <= 0)
            return (false, "Amount must be greater than zero.");

        await using var dbTransaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var demandAccount = await _context.Accounts
                .FirstOrDefaultAsync(x => x.UserId == userId && x.AccountType == "CURRENT");

            var timeDepositAccount = await _context.Accounts
                .FirstOrDefaultAsync(x => x.UserId == userId && x.AccountType == "TIME DEPOSIT");

            if (demandAccount == null)
                return (false, "Current account not found.");

            if (timeDepositAccount == null)
                return (false, "Time deposit account not found.");

            var demandBalance = demandAccount.Balance ?? 0m;

            if (demandBalance < amount)
                return (false, "Insufficient balance in your current account.");

            demandAccount.Balance = demandBalance - amount;
            timeDepositAccount.Balance = (timeDepositAccount.Balance ?? 0m) + amount;

            var transaction = new Transaction
            {
                SenderAccountId = demandAccount.AccountId,
                ReceiverAccountId = timeDepositAccount.AccountId,
                Amount = amount,
                TransactionType = "INTERNAL TRANSFER",
                Description = "Transfer from current account to time deposit account",
                CreatedAt = DateTime.Now
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _notificationService.CreateAsync(
                userId,
                $"{amount:N2} TL has been transferred from your current account to your time deposit account."
            ); 
            
            await dbTransaction.CommitAsync();

            return (true, "Funds successfully transferred from current account to time deposit account.");
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            return (false, "An unexpected error occurred during the transfer.");
        }
    }

    public async Task<(bool Success, string Message)> TransferTimeDepositToDemandAsync(int userId, decimal amount)
    {
        if (amount <= 0)
            return (false, "Amount must be greater than zero.");

        await using var dbTransaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var demandAccount = await _context.Accounts
                .FirstOrDefaultAsync(x => x.UserId == userId && x.AccountType == "CURRENT");

            var timeDepositAccount = await _context.Accounts
                .FirstOrDefaultAsync(x => x.UserId == userId && x.AccountType == "TIME DEPOSIT");

            if (demandAccount == null)
                return (false, "Current account not found.");

            if (timeDepositAccount == null)
                return (false, "Time deposit account not found.");

            var timeDepositBalance = timeDepositAccount.Balance ?? 0m;

            if (timeDepositBalance < amount)
                return (false, "Insufficient balance in your time deposit account.");

            timeDepositAccount.Balance = timeDepositBalance - amount;
            demandAccount.Balance = (demandAccount.Balance ?? 0m) + amount;

            var transaction = new Transaction
            {
                SenderAccountId = timeDepositAccount.AccountId,
                ReceiverAccountId = demandAccount.AccountId,
                Amount = amount,
                TransactionType = "INTERNAL TRANSFER",
                Description = "Transfer from time deposit account to current account",
                CreatedAt = DateTime.Now
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _notificationService.CreateAsync(
                userId,
                $"{amount:N2} TL has been transferred from your time deposit account to your current account."
            );

            await dbTransaction.CommitAsync();

            return (true, "Funds successfully transferred from time deposit account to current account.");
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            return (false, "An unexpected error occurred during the transfer.");
        }
    }

    private static string NormalizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return text.Trim()
            .ToUpperInvariant()
            .Replace("İ", "I")
            .Replace("Ş", "S")
            .Replace("Ğ", "G")
            .Replace("Ü", "U")
            .Replace("Ö", "O")
            .Replace("Ç", "C");
    }

    private static string NormalizeIban(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
            return string.Empty;

        return iban
            .Trim()
            .Replace(" ", "")
            .ToUpperInvariant();
    }

    public async Task<(bool Success, string Message)> TransferToAnotherUserByIbanAsync(
        int userId,
        decimal amount,
        string receiverIban,
        string receiverFirstName,
        string receiverLastName,
        string? description,
        int paymentTypeId)
    {
        // 🔹 1. BASIC VALIDATION
        if (amount <= 0)
            return (false, "Amount must be greater than zero.");

        receiverIban = NormalizeIban(receiverIban);
        receiverFirstName = receiverFirstName?.Trim() ?? string.Empty;
        receiverLastName = receiverLastName?.Trim() ?? string.Empty;
        description = description?.Trim();

        if (string.IsNullOrWhiteSpace(receiverIban))
            return (false, "Receiver IBAN is required.");

        if (string.IsNullOrWhiteSpace(receiverFirstName))
            return (false, "Receiver first name is required.");

        if (string.IsNullOrWhiteSpace(receiverLastName))
            return (false, "Receiver last name is required.");

        if (paymentTypeId <= 0)
            return (false, "Please select a valid transaction type.");

        var paymentType = await _context.PaymentTypes
            .FirstOrDefaultAsync(x => x.PaymentTypeId == paymentTypeId);

        if (paymentType == null)
            return (false, "Selected transaction type not found.");

        await using var dbTransaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var senderAccount = await _context.Accounts
                .FirstOrDefaultAsync(x => x.UserId == userId && x.AccountType == "CURRENT");

            if (senderAccount == null)
                return (false, "Sender current account not found.");

            var senderIban = NormalizeIban(senderAccount.Iban);

            if (senderIban == receiverIban)
                return (false, "You cannot send money to your own IBAN.");

            var receiverAccount = await _context.Accounts
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Iban != null && x.Iban.Replace(" ", "").ToUpper() == receiverIban);

            if (receiverAccount == null)
                return (false, "Receiver IBAN not found.");

            if (receiverAccount.UserId == userId)
                return (false, "You cannot perform this transaction to your own account from this screen.");

            if (receiverAccount.AccountId == senderAccount.AccountId)
                return (false, "Cannot transfer to the same account.");

            var dbFirstName = NormalizeText(receiverAccount.User?.UserName);
            var dbLastName = NormalizeText(receiverAccount.User?.UserSurname);

            var inputFirstName = NormalizeText(receiverFirstName);
            var inputLastName = NormalizeText(receiverLastName);

            if (dbFirstName != inputFirstName || dbLastName != inputLastName)
                return (false, "Receiver name and surname do not match with the IBAN information.");

            var senderBalance = senderAccount.Balance ?? 0m;
            if (senderBalance < amount)
                return (false, "Insufficient balance in current account.");

            senderAccount.Balance = senderBalance - amount;
            receiverAccount.Balance = (receiverAccount.Balance ?? 0m) + amount;

            var transferTransaction = new Transaction
            {
                SenderAccountId = senderAccount.AccountId,
                ReceiverAccountId = receiverAccount.AccountId,
                Amount = amount,
                TransactionType = paymentType.Name,
                Description = string.IsNullOrWhiteSpace(description)
                    ? "Money transfer to another user via IBAN"
                    : description,
                PaymentTypeId = paymentTypeId,
                CreatedAt = DateTime.Now
            };

            _context.Transactions.Add(transferTransaction);
            await _context.SaveChangesAsync();

            var senderUser = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userId);

            var senderFullName = $"{senderUser?.UserName} {senderUser?.UserSurname}".Trim();
            var receiverFullName = $"{receiverAccount.User?.UserName} {receiverAccount.User?.UserSurname}".Trim();

            var senderNotification = new Notification
            {
                UserId = senderAccount.UserId,
                TransactionId = transferTransaction.TransactionId,
                Message = $"You have sent {amount:N2} TL to {receiverFullName}.",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            var receiverNotification = new Notification
            {
                UserId = receiverAccount.UserId,
                TransactionId = transferTransaction.TransactionId,
                Message = $"You have received {amount:N2} TL from {senderFullName}.",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(senderNotification);
            _context.Notifications.Add(receiverNotification);

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return (true, "Transfer completed successfully.");
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            return (false, "An unexpected error occurred during the transfer.");
        }
    }

    public async Task ApplyDailyInterestIfNeededAsync(int userId)
    {
        var timeDepositAccount = await _context.Accounts
            .FirstOrDefaultAsync(x => x.UserId == userId && x.AccountType == "TIME DEPOSIT");

        if (timeDepositAccount == null)
            return;

        var balance = timeDepositAccount.Balance ?? 0m;
        var interestRate = DefaultTimeDepositInterestRate;

        if (balance <= 0 || interestRate <= 0)
            return;

        if (timeDepositAccount.LastInterestDate.HasValue &&
            timeDepositAccount.LastInterestDate.Value.AddHours(24) > DateTime.Now)
        {
            return;
        }

        var interestPaymentType = await _context.PaymentTypes
            .FirstOrDefaultAsync(x => x.Name == "INTEREST");

        decimal interestAmount = Math.Round(balance * (interestRate / 100m), 2, MidpointRounding.AwayFromZero);

        if (interestAmount <= 0)
            return;

        timeDepositAccount.Balance = balance + interestAmount;
        timeDepositAccount.LastInterestDate = DateTime.Now;
        timeDepositAccount.InterestRate = DefaultTimeDepositInterestRate;

        var transaction = new Transaction
        {
            SenderAccountId = null,
            ReceiverAccountId = timeDepositAccount.AccountId,
            Amount = interestAmount,
            TransactionType = "INTEREST",
            Description = "Daily interest earnings added to your time deposit account.",
            PaymentTypeId = interestPaymentType?.PaymentTypeId,
            CreatedAt = DateTime.Now
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var notification = new Notification
        {
            UserId = userId,
            TransactionId = transaction.TransactionId,
            Message = $"Daily interest of {interestAmount:N2} TL has been added to your time deposit account.",
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<AccountPageDto> GetAccountPageAsync(int userId)
    {
        var accounts = await _context.Accounts
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        var demandAccount = accounts.FirstOrDefault(x => x.AccountType == "CURRENT");
        var timeDepositAccount = accounts.FirstOrDefault(x => x.AccountType == "TIME DEPOSIT");

        var accountIds = accounts.Select(x => x.AccountId).ToList();

        var activeCards = await _context.Cards
            .Where(x => accountIds.Contains(x.AccountId) && x.IsActive == true)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var currentCardEntity = demandAccount == null
            ? null
            : activeCards.FirstOrDefault(x => x.AccountId == demandAccount.AccountId);

        var timeDepositCardEntity = timeDepositAccount == null
            ? null
            : activeCards.FirstOrDefault(x => x.AccountId == timeDepositAccount.AccountId);

        return new AccountPageDto
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

            CurrentCard = currentCardEntity == null ? null : new CardDto
            {
                CardId = currentCardEntity.CardId,
                AccountId = currentCardEntity.AccountId,
                CardNumber = currentCardEntity.CardNumber ?? string.Empty,
                ExpiryDate = currentCardEntity.ExpiryDate,
                Cvv = currentCardEntity.Cvv ?? string.Empty,
                IsActive = currentCardEntity.IsActive ?? false,
                CreatedAt = currentCardEntity.CreatedAt
            },

            TimeDepositCard = timeDepositCardEntity == null ? null : new CardDto
            {
                CardId = timeDepositCardEntity.CardId,
                AccountId = timeDepositCardEntity.AccountId,
                CardNumber = timeDepositCardEntity.CardNumber ?? string.Empty,
                ExpiryDate = timeDepositCardEntity.ExpiryDate,
                Cvv = timeDepositCardEntity.Cvv ?? string.Empty,
                IsActive = timeDepositCardEntity.IsActive ?? false,
                CreatedAt = timeDepositCardEntity.CreatedAt
            },

            HasTimeDepositAccount = timeDepositAccount != null
        };
    }
}