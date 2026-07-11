using Insight.Services.Interfaces.Core;
using Insight.WebApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Insight.Services.Core.Modules;

public class CoreModule : IModule
{
    public string ModuleName => "Core";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICitationExtractor, CitationExtractorService>();
        services.AddScoped<IContentQualityReviewer, ContentQualityReviewerService>();
        services.AddScoped<IBlogService, BlogService>();
    }
}
