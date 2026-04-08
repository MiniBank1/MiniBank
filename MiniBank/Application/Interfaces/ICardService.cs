using Application.DTOs;

namespace Application.Interfaces;

public interface ICardService
{
    Task<CardPageDto> GetMyCardPageAsync(int userId);

    Task<(bool Success, string Message)> CreateCardForCurrentAccountAsync(int userId);
    Task<(bool Success, string Message)> CreateCardForTimeDepositAccountAsync(int userId);
    Task<(bool Success, string Message)> CreateCardForAccountAsync(int accountId);
    Task<(bool Success, string Message)> DeactivateMyCardAsync(int userId);
    Task DeleteCardsByAccountIdAsync(int accountId);
}