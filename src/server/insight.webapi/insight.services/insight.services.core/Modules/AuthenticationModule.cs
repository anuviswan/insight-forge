using Insight.Services.Core.Domain.Services;
using Insight.Services.Core.Persistence;
using Insight.Services.Interfaces.Core;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Insight.Services.Core.Modules;

/// <summary>
/// Wrapper to hold both table clients.
/// </summary>
public class TableClientProvider
{
    public TableClient UsersTable { get; set; } = null!;
    public TableClient VerificationsTable { get; set; } = null!;
}

/// <summary>
/// Authentication and user management module.
/// Registers all authentication, password, JWT, email, and storage services.
/// </summary>
public class AuthenticationModule : IModule
{
    public string ModuleName => "Authentication";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure Azure Table Storage
        var storageConnectionString = configuration["AzureTableStorage:ConnectionString"];
        var usersTableName = configuration["AzureTableStorage:UsersTableName"] ?? "users";
        var verificationsTableName = configuration["AzureTableStorage:VerificationTableName"] ?? "emailverifications";

        if (string.IsNullOrEmpty(storageConnectionString))
            throw new InvalidOperationException("AzureTableStorage:ConnectionString not configured. Set environment variable AZURE_STORAGE_CONNECTION_STRING.");

        // Register Table Client Provider as singleton
        services.AddSingleton(sp =>
        {
            // Use TableServiceClient to manage table clients
            var serviceClient = new TableServiceClient(storageConnectionString);
            
            // Create tables if they don't exist
            try
            {
                serviceClient.CreateTableIfNotExistsAsync(usersTableName).Wait();
                serviceClient.CreateTableIfNotExistsAsync(verificationsTableName).Wait();
            }
            catch (Exception ex)
            {
                // Log but don't fail startup if tables can't be created
                // They might be created manually or have permission issues in some environments
                System.Diagnostics.Debug.WriteLine($"Warning: Could not ensure tables exist: {ex.Message}");
            }

            // Get table clients
            var usersClient = serviceClient.GetTableClient(usersTableName);
            var verificationsClient = serviceClient.GetTableClient(verificationsTableName);

            return new TableClientProvider
            {
                UsersTable = usersClient,
                VerificationsTable = verificationsClient
            };
        });

        // Register core services
        services.AddScoped<ITableStorageClient, AzureTableStorageClient>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddScoped<IUserService, UserService>();
    }
}
