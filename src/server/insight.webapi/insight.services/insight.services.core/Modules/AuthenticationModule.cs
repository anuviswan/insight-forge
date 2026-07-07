using Insight.Services.Core.Domain.Services;
using Insight.Services.Core.Persistence;
using Insight.Services.Interfaces.Core;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Insight.Services.Core.Modules;

/// <summary>
/// Wrapper to hold both table clients for DI.
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
            var usersClient = new TableClient(new Uri(storageConnectionString), usersTableName);
            usersClient.CreateTableIfNotExists();

            var verificationsClient = new TableClient(new Uri(storageConnectionString), verificationsTableName);
            verificationsClient.CreateTableIfNotExists();

            return new TableClientProvider
            {
                UsersTable = usersClient,
                VerificationsTable = verificationsClient
            };
        });

        // Register table clients from provider
        services.AddSingleton(sp => sp.GetRequiredService<TableClientProvider>().UsersTable);
        services.AddSingleton(sp =>
        {
            // Create a named instance - workaround for multiple registrations of same type
            return sp.GetRequiredService<TableClientProvider>().VerificationsTable;
        });

        // Register core services
        services.AddScoped<ITableStorageClient, AzureTableStorageClient>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddScoped<IUserService, UserService>();
    }
}
