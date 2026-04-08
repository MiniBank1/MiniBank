using Application.Interfaces;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendVerificationCodeAsync(string email, string code)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("NovaBank", _configuration["EmailSettings:From"]));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "NovaBank Registration Verification Code";
        message.Body = new TextPart("plain")
        {
            Text = $"Verification Code: {code}"
        };

        var host = _configuration["EmailSettings:SmtpHost"];
        var portText = _configuration["EmailSettings:Port"];
        var port = Convert.ToInt32(portText);
        var username = _configuration["EmailSettings:Username"];
        var password = _configuration["EmailSettings:Password"];

        using var client = new MailKit.Net.Smtp.SmtpClient();

        await client.ConnectAsync(host, 465, SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendPasswordResetLinkAsync(string email, string resetLink)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("NovaBank", _configuration["EmailSettings:From"]));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "NovaBank ";
        message.Body = new TextPart("plain")
        {
            Text = $"Click the link to reset your password:\n{resetLink}"
        };

        var host = _configuration["EmailSettings:SmtpHost"];
        var portText = _configuration["EmailSettings:Port"];
        var port = Convert.ToInt32(portText);
        var username = _configuration["EmailSettings:Username"];
        var password = _configuration["EmailSettings:Password"];

        using var client = new MailKit.Net.Smtp.SmtpClient();

        await client.ConnectAsync(host, 465, SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}