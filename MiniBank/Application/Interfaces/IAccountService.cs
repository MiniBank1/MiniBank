using Application.DTOs;

namespace Application.Interfaces;
using Application.DTOs;

public interface IAccountService
{
    Task<(bool Success, string Message)> CreateTimeDepositAccountAsync(int userId);
    Task<(bool Success, string Message)> DeleteTimeDepositAccountAsync(int userId);
    Task<(bool Success, string Message)> TransferDemandToTimeDepositAsync(int userId, decimal amount);
    Task<(bool Success, string Message)> TransferTimeDepositToDemandAsync(int userId, decimal amount);

    Task<(bool Success, string Message)> TransferToAnotherUserByIbanAsync(
        int userId,
        decimal amount,
        string receiverIban,
        string receiverFirstName,
        string receiverLastName,
        string? description,
        int paymentTypeId);

    Task ApplyDailyInterestIfNeededAsync(int userId);
    Task<AccountPageDto> GetAccountPageAsync(int userId);
}