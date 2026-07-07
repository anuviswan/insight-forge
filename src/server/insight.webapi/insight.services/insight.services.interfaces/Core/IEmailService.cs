namespace Insight.Services.Interfaces.Core;

/// <summary>
/// Service interface for sending emails.
/// Allows pluggable implementations (mock, SMTP, SendGrid, etc.)
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email verification message with confirmation link.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="verificationLink">Full URL to verification page with token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendVerificationEmailAsync(string email, string verificationLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send password reset email.
    /// </summary>
    Task SendPasswordResetEmailAsync(string email, string resetLink, CancellationToken cancellationToken = default);
}
