using Application.DTOs;
using Domain.Entity;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto);
    Task<(bool Success, string Message)> VerifyRegisterAsync(VerifyRegisterDto dto);
    Task<User?> LoginAsync(LoginDto dto);
    Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto dto, string resetBaseUrl);
    Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto);
}