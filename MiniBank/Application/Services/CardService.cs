using Application.DTOs;
using Application.Interfaces;
using Domain.Entity;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Application.Services;

public class CardService : ICardService
{
    private readonly MiniBankDbContext _context;
    private readonly INotificationService _notificationService;

    public CardService(
        MiniBankDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<CardPageDto> GetMyCardPageAsync(int userId)
    {
        var currentAccount = await _context.Accounts
            .FirstOrDefaultAsync(x => x.UserId == userId && x.AccountType == "CURRENT");

        if (currentAccount == null)
            return new CardPageDto();

        var activeCard = await _context.Cards
            .Where(x => x.AccountId == currentAccount.AccountId && x.IsActive == true)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new CardDto
            {
                CardId = x.CardId,
                AccountId = x.AccountId,
                CardNumber = x.CardNumber ?? string.Empty,
                ExpiryDate = x.ExpiryDate,
                Cvv = x.Cvv ?? string.Empty,
                IsActive = x.IsActive ?? false,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();

        return new CardPageDto
        {
            ActiveCard = activeCard
        };
    }

    public async Task<(bool Success, string Message)> CreateCardForCurrentAccountAsync(int userId)
    {
        var currentAccount = await _context.Accounts
            .FirstOrDefaultAsync(x => x.UserId == userId && x.AccountType == "CURRENT");

        if (currentAccount == null)
            return (false, "Current account not found.");

        return await CreateCardForAccountAsync(currentAccount.AccountId);
    }

    public async Task<(bool Success, string Message)> CreateCardForTimeDepositAccountAsync(int userId)
    {
        var timeDepositAccount = await _context.Accounts
            .FirstOrDefaultAsync(x => x.UserId == userId && x.AccountType == "TIME DEPOSIT");

        if (timeDepositAccount == null)
            return (false, "Time deposit account not found.");

        return await CreateCardForAccountAsync(timeDepositAccount.AccountId);
    }

    public async Task<(bool Success, string Message)> CreateCardForAccountAsync(int accountId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(x => x.AccountId == accountId);

        if (account == null)
            return (false, "Account not found.");

        var hasActiveCard = await _context.Cards
            .AnyAsync(x => x.AccountId == accountId && x.IsActive == true);

        if (hasActiveCard)
            return (false, "This account already has an active card.");

        var newCard = new Card
        {
            AccountId = accountId,
            CardNumber = await GenerateUniqueCardNumberAsync(),
            ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(5)),
            Cvv = GenerateCvv(),
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _context.Cards.Add(newCard);
        await _context.SaveChangesAsync();

        await _notificationService.CreateAsync(
            account.UserId,
            $"A new card has been created for your {account.AccountType} account."
        );

        return (true, "Card created successfully.");
    }

    public async Task<(bool Success, string Message)> DeactivateMyCardAsync(int userId)
    {
        var currentAccount = await _context.Accounts
            .FirstOrDefaultAsync(x => x.UserId == userId && x.AccountType == "CURRENT");

        if (currentAccount == null)
            return (false, "Current account not found.");

        var activeCard = await _context.Cards
            .Where(x => x.AccountId == currentAccount.AccountId && x.IsActive == true)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (activeCard == null)
            return (false, "Active card not found.");

        activeCard.IsActive = false;
        await _context.SaveChangesAsync();

        await _notificationService.CreateAsync(
            userId,
            "Your current account card has been deactivated successfully."
        );

        return (true, "Card deactivated successfully.");
    }

    public async Task DeleteCardsByAccountIdAsync(int accountId)
    {
        var cards = await _context.Cards
            .Where(x => x.AccountId == accountId)
            .ToListAsync();

        if (cards.Any())
        {
            _context.Cards.RemoveRange(cards);
            await _context.SaveChangesAsync();
        }
    }

    private async Task<string> GenerateUniqueCardNumberAsync()
    {
        string cardNumber;
        bool exists;

        do
        {
            cardNumber = Generate16DigitCardNumber();
            exists = await _context.Cards.AnyAsync(x => x.CardNumber == cardNumber);
        }
        while (exists);

        return cardNumber;
    }

    private string Generate16DigitCardNumber()
    {
        Span<byte> buffer = stackalloc byte[16];
        RandomNumberGenerator.Fill(buffer);

        char[] digits = new char[16];
        for (int i = 0; i < 16; i++)
        {
            digits[i] = (char)('0' + (buffer[i] % 10));
        }

        if (digits[0] == '0')
            digits[0] = '4';

        return new string(digits);
    }

    private string GenerateCvv()
    {
        int cvv = RandomNumberGenerator.GetInt32(100, 1000);
        return cvv.ToString();
    }
}