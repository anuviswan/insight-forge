
using System;
using insight.webapi.Services;
using Microsoft.Extensions.DependencyInjection;

namespace insight.webapi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Antigravity HttpClient and service registrations
            builder.Services.AddHttpClient<IAntigravityApiClient, AntigravityApiClient>(client =>
            {
                var baseUrl = builder.Configuration["Antigravity:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/";
                client.BaseAddress = new Uri(baseUrl);
            });

            // Use built-in OpenAPI provider (Microsoft.AspNetCore.OpenApi)
            // Do not register Swashbuckle when using the built-in OpenAPI to avoid type conflicts.

            // Use built-in OpenAPI mapping (AddOpenApi/MapOpenApi) for minimal OpenAPI support

            builder.Services.AddScoped<IAgentMetadataProvider, FileAgentMetadataProvider>();
            builder.Services.AddScoped<IAntigravityAgent, AntigravityAgent>();
            builder.Services.AddScoped<IBlogService, BlogService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // Map OpenAPI endpoints (Swagger/OpenAPI) and enable Swagger UI
            // Map built-in OpenAPI endpoints. Use the built-in document at /openapi.
            app.MapOpenApi();
            // If you want a UI, add a Swagger UI package or host your own UI that points to /openapi.

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
