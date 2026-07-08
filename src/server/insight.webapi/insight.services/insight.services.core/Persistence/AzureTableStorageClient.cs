using Insight.Services.Core.Domain.Entities;
using Insight.Services.Interfaces.Core;
using Insight.Services.Core.Modules;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

namespace Insight.Services.Core.Persistence;

/// <summary>
/// Azure Table Storage implementation for user and verification data.
/// Provides simple, direct table operations without ORM overhead.
/// </summary>
public class AzureTableStorageClient : ITableStorageClient
{
    private readonly TableClient _usersTable;
    private readonly TableClient _verificationsTable;
    private readonly ILogger<AzureTableStorageClient> _logger;

    public AzureTableStorageClient(
        TableClientProvider tableClientProvider,
        ILogger<AzureTableStorageClient> logger)
    {
        _usersTable = tableClientProvider.UsersTable;
        _verificationsTable = tableClientProvider.VerificationsTable;
        _logger = logger;
    }

    public async Task<object?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _usersTable.GetEntityAsync<UserEntity>("Users", userId, cancellationToken: cancellationToken);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by ID {UserId}", userId);
            throw;
        }
    }

    public async Task<object?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _usersTable.QueryAsync<UserEntity>(
                u => u.PartitionKey == "Users" && u.Email == email,
                cancellationToken: cancellationToken);

            var result = await query.FirstOrDefaultAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email {Email}", email);
            throw;
        }
    }

    public async Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByEmailAsync(email, cancellationToken);
        return user != null;
    }

    public async Task CreateUserAsync(object user, CancellationToken cancellationToken = default)
    {
        if (user is not UserEntity userEntity)
            throw new ArgumentException("User must be UserEntity", nameof(user));

        try
        {
            if (string.IsNullOrEmpty(userEntity.RowKey))
                userEntity.RowKey = Guid.NewGuid().ToString();

            userEntity.PartitionKey = "Users";
            userEntity.CreatedAt = DateTime.UtcNow;

            await _usersTable.AddEntityAsync(userEntity, cancellationToken: cancellationToken);
            _logger.LogInformation("User created: {UserId} ({Email})", userEntity.RowKey, userEntity.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", userEntity.Email);
            throw;
        }
    }

    public async Task UpdateUserAsync(object user, CancellationToken cancellationToken = default)
    {
        if (user is not UserEntity userEntity)
            throw new ArgumentException("User must be UserEntity", nameof(user));

        try
        {
            await _usersTable.UpdateEntityAsync(userEntity, userEntity.ETag, TableUpdateMode.Replace, cancellationToken: cancellationToken);
            _logger.LogInformation("User updated: {UserId}", userEntity.RowKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userEntity.RowKey);
            throw;
        }
    }

    public async Task CreateVerificationTokenAsync(object verification, CancellationToken cancellationToken = default)
    {
        if (verification is not EmailVerificationEntity verificationEntity)
            throw new ArgumentException("Verification must be EmailVerificationEntity", nameof(verification));

        try
        {
            verificationEntity.CreatedAt = DateTime.UtcNow;
            await _verificationsTable.AddEntityAsync(verificationEntity, cancellationToken: cancellationToken);
            _logger.LogInformation("Verification token created for user {UserId}", verificationEntity.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating verification token for user {UserId}", verificationEntity.UserId);
            throw;
        }
    }

    public async Task<object?> GetVerificationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _verificationsTable.QueryAsync<EmailVerificationEntity>(
                v => v.RowKey == token,
                cancellationToken: cancellationToken);

            var result = await query.FirstOrDefaultAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving verification token");
            throw;
        }
    }

    public async Task UpdateVerificationTokenAsync(object verification, CancellationToken cancellationToken = default)
    {
        if (verification is not EmailVerificationEntity verificationEntity)
            throw new ArgumentException("Verification must be EmailVerificationEntity", nameof(verification));

        try
        {
            await _verificationsTable.UpdateEntityAsync(verificationEntity, verificationEntity.ETag, TableUpdateMode.Replace, cancellationToken: cancellationToken);
            _logger.LogInformation("Verification token updated for user {UserId}", verificationEntity.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating verification token");
            throw;
        }
    }
}
