using Insight.Services.Interfaces.Core;
using Microsoft.Extensions.Logging;

namespace Insight.Services.Core.Domain.Services;

/// <summary>
/// Mock email service for development and testing.
/// Logs email sends instead of actually sending.
/// </summary>
public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendVerificationEmailAsync(string email, string verificationLink, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "MOCK EMAIL: Verification email would be sent to {Email}\n" +
            "Verification Link: {VerificationLink}",
            email,
            verificationLink
        );

        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string email, string resetLink, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "MOCK EMAIL: Password reset email would be sent to {Email}\n" +
            "Reset Link: {ResetLink}",
            email,
            resetLink
        );

        return Task.CompletedTask;
    }
}
