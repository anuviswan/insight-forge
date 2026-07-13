namespace Insight.Services.Interfaces.Ai.Events;

public class ProgressData
{
    public int CurrentStep { get; set; }
    public int? TotalSteps { get; set; }
    public int WordCount { get; set; }
    public int CharacterCount { get; set; }
    public int ParagraphCount { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public long? TotalInputTokens { get; set; }
    public long? TotalOutputTokens { get; set; }
    public long? TotalCachedTokens { get; set; }
    public long? TotalThoughtTokens { get; set; }
}
