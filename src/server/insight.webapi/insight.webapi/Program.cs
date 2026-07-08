using Insight.Services.Core.Configuration;
using Insight.Services.Interfaces.Core;
using Insight.WebApi.Services;
using System.Diagnostics;

namespace Insight.WebApi;

public class Program
{
    private static Process? _azuriteProcess;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddCors(options =>
        {
            if (builder.Environment.IsDevelopment())
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            }
            else
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            }
        });

        RegisterModules(builder.Services, builder.Configuration);
        RegisterAuthenticationServices(builder.Services, builder.Configuration);
        builder.Services.AddScoped<IBlogService, BlogService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            StartAzurite(app.Services.GetRequiredService<ILogger<Program>>());

            app.Lifetime.ApplicationStopping.Register(() =>
            {
                StopAzurite();
            });
        }

        app.MapOpenApi();
        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    private static void StartAzurite(ILogger<Program> logger)
    {
        try
        {
            var azuriteProcess = Process.GetProcessesByName("azurite").FirstOrDefault();
            if (azuriteProcess != null)
            {
                logger.LogInformation("Azurite is already running (PID: {ProcessId})", azuriteProcess.Id);
                return;
            }

            logger.LogInformation("Starting Azurite...");

            var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
            var fileName = isWindows ? "cmd.exe" : "/bin/bash";
            var arguments = isWindows
                ? "/c npx azurite --silent --location ./azurite-data"
                : "-c \"npx azurite --silent --location ./azurite-data\"";

            _azuriteProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                }
            };

            // Ensure PATH is available for npm
            if (!_azuriteProcess.StartInfo.EnvironmentVariables.ContainsKey("PATH"))
            {
                var pathEnv = Environment.GetEnvironmentVariable("PATH");
                if (!string.IsNullOrEmpty(pathEnv))
                {
                    _azuriteProcess.StartInfo.EnvironmentVariables["PATH"] = pathEnv;
                }
            }

            _azuriteProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    logger.LogInformation("Azurite: {Message}", e.Data);
                }
            };

            _azuriteProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    logger.LogWarning("Azurite: {Message}", e.Data);
                }
            };

            _azuriteProcess.Start();
            _azuriteProcess.BeginOutputReadLine();
            _azuriteProcess.BeginErrorReadLine();

            System.Threading.Thread.Sleep(2000);
            logger.LogInformation("Azurite started successfully (PID: {ProcessId})", _azuriteProcess.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start Azurite. Make sure Node.js and npm are installed. Install with: npm install -g azurite");
        }
    }

    private static void StopAzurite()
    {
        if (_azuriteProcess != null && !_azuriteProcess.HasExited)
        {
            try
            {
                _azuriteProcess.Kill();
                _azuriteProcess.WaitForExit(5000);
                _azuriteProcess.Dispose();
            }
            catch
            {
                // Suppress errors during cleanup
            }
        }
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
