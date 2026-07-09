namespace Insight.Services.Interfaces.Core;

public interface IResearchService
{
    Task<string> ConductResearchAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default);
}
