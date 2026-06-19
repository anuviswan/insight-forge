
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
                var baseUrl = builder.Configuration["Antigravity:BaseUrl"] ?? "https://api.antigravity.dev/";
                client.BaseAddress = new Uri(baseUrl);
            });

            builder.Services.AddScoped<IAgentMetadataProvider, FileAgentMetadataProvider>();
            builder.Services.AddScoped<IAntigravityAgent, AntigravityAgent>();
            builder.Services.AddScoped<IBlogService, BlogService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
