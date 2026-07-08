using Insight.Services.Core.Configuration;
using Insight.Services.Core.Modules;
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
            new AuthenticationModule(),
            new global::Insight.Services.Ai.Gemini.GeminiModule(),
        };

        foreach (var module in modules)
        {
            module.RegisterServices(services, configuration);
        }
    }

}
