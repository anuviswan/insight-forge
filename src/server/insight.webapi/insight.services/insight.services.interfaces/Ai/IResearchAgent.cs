namespace Insight.Services.Interfaces.Ai;

public interface IResearchAgent
{
    Task<string> ConductResearchAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default);
}
