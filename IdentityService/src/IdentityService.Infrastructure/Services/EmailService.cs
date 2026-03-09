using IdentityService.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace IdentityService.Infrastructure.Services;

/// <summary>
/// Sends transactional emails via Gmail SMTP using MailKit.
/// Configuration is read from the "EmailSettings" section in appsettings.json.
/// No credentials are hard-coded in source code.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string toEmail, string toName, string otp)
    {
        var settings = _configuration.GetSection("EmailSettings");

        var fromEmail = settings["FromEmail"]
            ?? throw new InvalidOperationException("EmailSettings:FromEmail is not configured.");
        var fromName = settings["FromName"] ?? "Bach Hoa Xanh Security";
        var smtpHost = settings["SmtpHost"]
            ?? throw new InvalidOperationException("EmailSettings:SmtpHost is not configured.");
        var smtpPort = int.Parse(settings["SmtpPort"] ?? "587");
        var smtpUser = settings["SmtpUser"]
            ?? throw new InvalidOperationException("EmailSettings:SmtpUser is not configured.");
        var smtpPassword = settings["SmtpPassword"]
            ?? throw new InvalidOperationException("EmailSettings:SmtpPassword is not configured.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = "Your OTP Code – Bach Hoa Xanh Security";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = BuildHtmlBody(toName, otp),
            TextBody = BuildTextBody(toName, otp)
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        try
        {
            // StartTls is the standard for Gmail port 587.
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("OTP email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendPasswordResetOtpEmailAsync(string toEmail, string toName, string otp)
    {
        var settings = _configuration.GetSection("EmailSettings");

        var fromEmail = settings["FromEmail"]
            ?? throw new InvalidOperationException("EmailSettings:FromEmail is not configured.");
        var fromName = settings["FromName"] ?? "Identity Service";
        var smtpHost = settings["SmtpHost"]
            ?? throw new InvalidOperationException("EmailSettings:SmtpHost is not configured.");
        var smtpPort = int.Parse(settings["SmtpPort"] ?? "587");
        var smtpUser = settings["SmtpUser"]
            ?? throw new InvalidOperationException("EmailSettings:SmtpUser is not configured.");
        var smtpPassword = settings["SmtpPassword"]
            ?? throw new InvalidOperationException("EmailSettings:SmtpPassword is not configured.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = "Password Reset OTP – Identity Service";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = BuildPasswordResetHtmlBody(toName, otp),
            TextBody = BuildPasswordResetTextBody(toName, otp)
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Password reset OTP email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset OTP email to {Email}", toEmail);
            throw;
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static string BuildHtmlBody(string name, string otp) => $"""
        <!DOCTYPE html>
        <html lang="en">
        <head><meta charset="UTF-8" /></head>
        <body style="font-family: Arial, sans-serif; background: #f4f4f4; padding: 30px;">
          <div style="max-width: 480px; margin: 0 auto; background: #ffffff;
                      border-radius: 8px; padding: 32px; box-shadow: 0 2px 8px rgba(0,0,0,.1);">
            <h2 style="color: #333;">Email Verification</h2>
            <p style="color: #555;">Hi <strong>{name}</strong>,</p>
            <p style="color: #555;">
              Use the OTP below to complete your registration.
              This code is valid for <strong>5 minutes</strong> and can only be used once.
            </p>
            <div style="text-align: center; margin: 24px 0;">
              <span style="font-size: 36px; font-weight: bold; letter-spacing: 8px;
                           color: #4F46E5; background: #EEF2FF; padding: 12px 24px;
                           border-radius: 8px;">{otp}</span>
            </div>
            <p style="color: #888; font-size: 13px;">
              If you did not request this code, please ignore this email.
            </p>
          </div>
        </body>
        </html>
        """;

    private static string BuildTextBody(string name, string otp) =>
        $"Hi {name},\n\nYour OTP code is: {otp}\n\n" +
        "This code expires in 5 minutes and can only be used once.\n\n" +
        "If you did not request this, ignore this email.";

    private static string BuildPasswordResetHtmlBody(string name, string otp) => $"""
        <!DOCTYPE html>
        <html lang="en">
        <head><meta charset="UTF-8" /></head>
        <body style="font-family: Arial, sans-serif; background: #f4f4f4; padding: 30px;">
          <div style="max-width: 480px; margin: 0 auto; background: #ffffff;
                      border-radius: 8px; padding: 32px; box-shadow: 0 2px 8px rgba(0,0,0,.1);">
            <h2 style="color: #333;">Password Reset Request</h2>
            <p style="color: #555;">Hi <strong>{name}</strong>,</p>
            <p style="color: #555;">
              We received a request to reset your password.
              Use the OTP below to proceed. This code is valid for <strong>15 minutes</strong> and can only be used once.
            </p>
            <div style="text-align: center; margin: 24px 0;">
              <span style="font-size: 36px; font-weight: bold; letter-spacing: 8px;
                           color: #DC2626; background: #FEF2F2; padding: 12px 24px;
                           border-radius: 8px;">{otp}</span>
            </div>
            <p style="color: #888; font-size: 13px;">
              If you did not request a password reset, please ignore this email. Your password will not change.
            </p>
          </div>
        </body>
        </html>
        """;

    private static string BuildPasswordResetTextBody(string name, string otp) =>
        $"Hi {name},\n\nYour password reset OTP code is: {otp}\n\n" +
        "This code expires in 15 minutes and can only be used once.\n\n" +
        "If you did not request this, ignore this email. Your password will not change.";
}
