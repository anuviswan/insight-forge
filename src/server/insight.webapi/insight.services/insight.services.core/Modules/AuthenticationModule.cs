using Insight.Services.Core.Domain.Services;
using Insight.Services.Core.Options;
using Insight.Services.Core.Persistence;
using Insight.Services.Interfaces.Core;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Insight.Services.Core.Modules;

/// <summary>
/// Wrapper to hold all table clients.
/// </summary>
public class TableClientProvider
{
    public TableClient UsersTable { get; set; } = null!;
    public TableClient VerificationsTable { get; set; } = null!;
    public TableClient LoginAttemptsTable { get; set; } = null!;
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
        // Configure LoginAttemptOptions from appsettings
        var section = configuration.GetSection(LoginAttemptOptions.SectionName);
        services.Configure<LoginAttemptOptions>(options =>
        {
            options.MaxFailedAttempts = int.TryParse(section["MaxFailedAttempts"], out var max) ? max : 5;
            options.LockoutDurationMinutes = int.TryParse(section["LockoutDurationMinutes"], out var lockout) ? lockout : 30;
        });

        // Configure Azure Table Storage
        var storageConnectionString = configuration["AzureTableStorage:ConnectionString"];
        var usersTableName = configuration["AzureTableStorage:UsersTableName"] ?? "users";
        var verificationsTableName = configuration["AzureTableStorage:VerificationTableName"] ?? "emailverifications";
        var loginAttemptsTableName = configuration["AzureTableStorage:LoginAttemptsTableName"] ?? "loginattempts";

        if (string.IsNullOrEmpty(storageConnectionString))
        {
            throw new InvalidOperationException(
                "AzureTableStorage:ConnectionString not configured.\n" +
                "For development: Use Azure Storage Emulator connection string in appsettings.Development.json\n" +
                "For production: Set environment variable AZURE_STORAGE_CONNECTION_STRING\n" +
                "Emulator connection string: DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXOU+FxsxvpT1+c=;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
            );
        }

        // Register Table Client Provider as singleton
        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<AuthenticationModule>>();
            
            try
            {
                // Use TableServiceClient to manage table clients
                var serviceClient = new TableServiceClient(storageConnectionString);
                
                // Create tables if they don't exist
                try
                {
                    serviceClient.CreateTableIfNotExistsAsync(usersTableName).Wait();
                    serviceClient.CreateTableIfNotExistsAsync(verificationsTableName).Wait();
                    serviceClient.CreateTableIfNotExistsAsync(loginAttemptsTableName).Wait();
                    logger.LogInformation("Azure Table Storage tables initialized: {UsersTable}, {VerificationsTable}, {LoginAttemptsTable}", usersTableName, verificationsTableName, loginAttemptsTableName);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Warning: Could not ensure tables exist. They may need to be created manually or permissions may be restricted.");
                }

                // Get table clients
                var usersClient = serviceClient.GetTableClient(usersTableName);
                var verificationsClient = serviceClient.GetTableClient(verificationsTableName);
                var loginAttemptsClient = serviceClient.GetTableClient(loginAttemptsTableName);

                return new TableClientProvider
                {
                    UsersTable = usersClient,
                    VerificationsTable = verificationsClient,
                    LoginAttemptsTable = loginAttemptsClient
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize Azure Table Storage. Ensure connection string is valid and storage account is accessible.");
                throw;
            }
        });

        // Register core services
        services.AddScoped<ITableStorageClient, AzureTableStorageClient>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddScoped<ILoginAttemptService, LoginAttemptService>();
        services.AddScoped<IUserService, UserService>();
    }
}
