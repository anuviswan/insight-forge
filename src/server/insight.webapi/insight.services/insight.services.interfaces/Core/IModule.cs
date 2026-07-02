using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Insight.Services.Interfaces.Core;

/// <summary>
/// Defines a module that can register its own services with the DI container.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Gets the name/key of the module.
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Register module-specific services.
    /// </summary>
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
}
