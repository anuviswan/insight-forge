namespace Insight.WebApi.Models;

/// <summary>
/// Response returned when a blog generation job is started asynchronously.
/// </summary>
public class BlogJobStartedResponse
{
    public string JobId { get; set; } = string.Empty;
}
