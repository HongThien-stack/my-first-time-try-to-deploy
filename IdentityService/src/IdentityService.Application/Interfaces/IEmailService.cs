namespace IdentityService.Application.Interfaces;

/// <summary>
/// Contract for sending transactional emails (OTP, notifications, etc.).
/// The concrete implementation lives in Infrastructure and uses MailKit.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a 6-digit OTP verification code to the specified email address.
    /// </summary>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="toName">Recipient display name (used in the email body).</param>
    /// <param name="otp">The plain-text 6-digit OTP to embed in the email.</param>
    Task SendOtpEmailAsync(string toEmail, string toName, string otp);

    /// <summary>
    /// Sends a 6-digit OTP password reset code to the specified email address.
    /// </summary>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="toName">Recipient display name (used in the email body).</param>
    /// <param name="otp">The plain-text 6-digit OTP to embed in the email.</param>
    Task SendPasswordResetOtpEmailAsync(string toEmail, string toName, string otp);
}
