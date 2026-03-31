using Microsoft.Extensions.Configuration;
using Sabanda.Application.Common.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Sabanda.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        _apiKey = configuration["SendGrid:ApiKey"] ?? throw new InvalidOperationException("SendGrid:ApiKey not configured.");
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@sabanda.app";
        _fromName = configuration["SendGrid:FromName"] ?? "Sabanda";
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(_fromEmail, _fromName);
        var toAddress = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, body, body);
        await client.SendEmailAsync(msg);
    }
}
