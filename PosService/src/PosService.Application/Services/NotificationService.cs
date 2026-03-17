using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace PosService.Application.Services;

public class NotificationService : INotificationService
{
    private static readonly Regex SmsRegex = new("^\\+?[1-9]\\d{8,14}$", RegexOptions.Compiled);
    private readonly EmailSettingsDto _emailSettings;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
    {
        var section = configuration.GetSection("EmailSettings");
        _emailSettings = new EmailSettingsDto
        {
            FromEmail = section["FromEmail"] ?? string.Empty,
            FromName = section["FromName"] ?? string.Empty,
            SmtpHost = section["SmtpHost"] ?? string.Empty,
            SmtpPort = int.TryParse(section["SmtpPort"], out var port) ? port : 587,
            SmtpUser = section["SmtpUser"] ?? string.Empty,
            SmtpPassword = section["SmtpPassword"] ?? string.Empty,
            EnableSsl = !bool.TryParse(section["EnableSsl"], out var enableSsl) || enableSsl
        };
        _logger = logger;
    }

    public async Task SendReceiptAsync(
        string method,
        string destination,
        ReceiptResponseDto receipt,
        byte[]? pdfAttachment,
        CancellationToken cancellationToken = default)
    {
        if (string.Equals(method, "EMAIL", StringComparison.OrdinalIgnoreCase))
        {
            if (pdfAttachment == null || pdfAttachment.Length == 0)
            {
                throw new ArgumentException("PDF attachment is required for EMAIL receipt sending.", nameof(pdfAttachment));
            }

            ValidateEmailSettings();

            using var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = $"Receipt {receipt.SaleNumber}",
                Body = BuildEmailBody(receipt),
                IsBodyHtml = false
            };

            message.To.Add(destination);

            using var pdfStream = new MemoryStream(pdfAttachment);
            var attachment = new Attachment(pdfStream, $"receipt-{receipt.SaleNumber}.pdf", "application/pdf");
            message.Attachments.Add(attachment);

            using var smtpClient = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPassword)
            };

            await smtpClient.SendMailAsync(message);

            _logger.LogInformation(
                "Sending receipt via EMAIL to {Destination}. SaleNumber: {SaleNumber}. AttachmentBytes: {AttachmentBytes}",
                destination,
                receipt.SaleNumber,
                pdfAttachment?.Length ?? 0);

            return;
        }

        if (string.Equals(method, "SMS", StringComparison.OrdinalIgnoreCase))
        {
            if (!SmsRegex.IsMatch(destination))
            {
                throw new ArgumentException("Destination phone number is invalid for SMS method.", nameof(destination));
            }

            var summary =
                $"Thank you for your purchase. Receipt {receipt.SaleNumber}. Total: {receipt.Total:N0} VND.";

            _logger.LogInformation(
                "Sending receipt summary via SMS to {Destination}. Content: {Summary}",
                destination,
                summary);

            return;
        }

        throw new ArgumentException($"Unsupported method: {method}", nameof(method));
    }

    private void ValidateEmailSettings()
    {
        if (string.IsNullOrWhiteSpace(_emailSettings.FromEmail) ||
            string.IsNullOrWhiteSpace(_emailSettings.SmtpHost) ||
            string.IsNullOrWhiteSpace(_emailSettings.SmtpUser) ||
            string.IsNullOrWhiteSpace(_emailSettings.SmtpPassword))
        {
            throw new InvalidOperationException("EmailSettings is not fully configured for SMTP sending.");
        }
    }

    private static string BuildEmailBody(ReceiptResponseDto receipt)
    {
        return $"Thank you for your purchase.\n" +
               $"Receipt: {receipt.SaleNumber}\n" +
               $"Date: {receipt.SaleDate:yyyy-MM-dd HH:mm:ss}\n" +
               $"Total: {receipt.Total:N0} VND\n" +
               "Your receipt PDF is attached in this email.";
    }
}
