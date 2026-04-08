namespace Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeAsync(string email, string code);
    Task SendPasswordResetLinkAsync(string email, string resetLink);
}
