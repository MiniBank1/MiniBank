using Application.DTOs;
using Application.Interfaces;
using BCrypt.Net;
using Domain.Entity;
using Infrastructure.Data;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly MiniBankDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IMemoryCache _memoryCache;

    public AuthService(
        MiniBankDbContext context,
        IEmailService emailService,
        IMemoryCache memoryCache)
    {
        _context = context;
        _emailService = emailService;
        _memoryCache = memoryCache;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            return (false, "Passwords do not match.");

        var emailExists = await _context.Users.AnyAsync(x => x.Email == dto.Email);
        if (emailExists)
            return (false, "This email is already registered.");

        var tcExists = await _context.Users.AnyAsync(x => x.TcNo == dto.TcNo);
        if (tcExists)
            return (false, "This ID number is already registered.");

        var code = new Random().Next(100000, 999999).ToString();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var pendingData = new PendingRegisterData
        {
            UserName = dto.UserName,
            UserSurname = dto.UserSurname,
            TcNo = dto.TcNo,
            Email = dto.Email,
            PasswordHash = passwordHash,
            Code = code
        };

        _memoryCache.Set(
            $"register:{dto.Email}",
            pendingData,
            TimeSpan.FromMinutes(10));

        await _emailService.SendVerificationCodeAsync(dto.Email, code);

        return (true, "A verification code has been sent to your email address.");
    }

    public async Task<(bool Success, string Message)> VerifyRegisterAsync(VerifyRegisterDto dto)
    {
        var cacheKey = $"register:{dto.Email}";
        var pending = _memoryCache.Get<PendingRegisterData>(cacheKey);

        if (pending == null)
            return (false, "The code has expired or registration data could not be found.");

        if (pending.Code != dto.Code)
            return (false, "Invalid verification code.");

        var emailExists = await _context.Users.AnyAsync(x => x.Email == pending.Email);
        if (emailExists)
            return (false, "This email is already registered.");

        var tcExists = await _context.Users.AnyAsync(x => x.TcNo == pending.TcNo);
        if (tcExists)
            return (false, "This ID number is already registered.");

        var user = new User
        {
            UserName = pending.UserName,
            UserSurname = pending.UserSurname,
            TcNo = pending.TcNo,
            Email = pending.Email,
            PasswordHash = pending.PasswordHash,
            CreatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _memoryCache.Remove(cacheKey);

        return (true, "Registration completed successfully.");
    }

    

    public async Task<User?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
        if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            return null;

        var isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        return isValid ? user : null;
    }

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto dto, string resetBaseUrl)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
        if (user == null)
            return (false, "No user found with this email address.");

        var token = Guid.NewGuid().ToString();

        var resetData = new ResetPasswordData
        {
            UserId = user.UserId,
            Email = user.Email!
        };

        _memoryCache.Set(
            $"reset:{token}",
            resetData,
            TimeSpan.FromMinutes(30));

        var resetLink = $"{resetBaseUrl}?token={token}";
        await _emailService.SendPasswordResetLinkAsync(dto.Email, resetLink);

        return (true, "A password reset link has been sent to your email address.");
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            return (false, "Passwords do not match.");

        var cacheKey = $"reset:{dto.Token}";
        var resetData = _memoryCache.Get<ResetPasswordData>(cacheKey);

        if (resetData == null)
            return (false, "Invalid or expired reset link.");

        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == resetData.UserId);
        if (user == null)
            return (false, "User not found.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();

        _memoryCache.Remove(cacheKey);

        return (true, "Your password has been updated successfully.");
    }


}
