using Insight.Services.Core.Configuration;
using Insight.Services.Interfaces.Core;
using Insight.WebApi.Services;

namespace Insight.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        RegisterModules(builder.Services, builder.Configuration);
        RegisterAuthenticationServices(builder.Services, builder.Configuration);
        builder.Services.AddScoped<IBlogService, BlogService>();

        var app = builder.Build();

        app.MapOpenApi();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    private static void RegisterModules(IServiceCollection services, IConfiguration configuration)
    {
        var modules = new IModule[]
        {
            new global::Insight.Services.Ai.Gemini.GeminiModule(),
        };

        foreach (var module in modules)
        {
            module.RegisterServices(services, configuration);
        }
    }

    private static void RegisterAuthenticationServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var storageConnectionString = configuration["AzureTableStorage:ConnectionString"];
        var usersTableName = configuration["AzureTableStorage:UsersTableName"] ?? "users";
        var verificationsTableName = configuration["AzureTableStorage:VerificationTableName"] ?? "emailverifications";

        if (string.IsNullOrEmpty(storageConnectionString))
        {
            throw new InvalidOperationException(
                "AzureTableStorage:ConnectionString not configured.\n" +
                "For development: Use Azure Storage Emulator connection string in appsettings.Development.json\n" +
                "For production: Set environment variable AZURE_STORAGE_CONNECTION_STRING\n" +
                "Emulator connection string: DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXOU+FxsxvpT1+c=;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
            );
        }

        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();

            try
            {
                var serviceClient = new Azure.Data.Tables.TableServiceClient(storageConnectionString);

                try
                {
                    serviceClient.CreateTableIfNotExistsAsync(usersTableName).Wait();
                    serviceClient.CreateTableIfNotExistsAsync(verificationsTableName).Wait();
                    logger.LogInformation("Azure Table Storage tables initialized: {UsersTable}, {VerificationsTable}", usersTableName, verificationsTableName);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Warning: Could not ensure tables exist. They may need to be created manually or permissions may be restricted.");
                }

                var usersClient = serviceClient.GetTableClient(usersTableName);
                var verificationsClient = serviceClient.GetTableClient(verificationsTableName);

                return new global::Insight.Services.Core.Modules.TableClientProvider
                {
                    UsersTable = usersClient,
                    VerificationsTable = verificationsClient
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize Azure Table Storage. Ensure connection string is valid and storage account is accessible.");
                throw;
            }
        });

        services.AddScoped<global::Insight.Services.Interfaces.Core.ITableStorageClient, global::Insight.Services.Core.Persistence.AzureTableStorageClient>();
        services.AddScoped<global::Insight.Services.Interfaces.Core.IPasswordService, global::Insight.Services.Core.Domain.Services.PasswordService>();
        services.AddScoped<global::Insight.Services.Interfaces.Core.IJwtTokenService, global::Insight.Services.Core.Domain.Services.JwtTokenService>();
        services.AddScoped<global::Insight.Services.Interfaces.Core.IEmailService, global::Insight.Services.Core.Domain.Services.MockEmailService>();
        services.AddScoped<global::Insight.Services.Interfaces.Core.IUserService, global::Insight.Services.Core.Domain.Services.UserService>();
    }
}
