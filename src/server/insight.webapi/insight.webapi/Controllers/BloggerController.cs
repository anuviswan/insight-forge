using Insight.Services.Interfaces.Core;
using Insight.WebApi.Models;
using Insight.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace insight.webapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BloggerController(IBlogService blogService) : ControllerBase
{
    [HttpPost("CreateBlogEntry")]
    public async Task<IActionResult> CreateBlogEntry([FromBody] CreateBlogRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Topic))
            return BadRequest("Topic is required.");

        var blogEntry = await blogService.CreateBlogEntryAsync(request.Topic, request.Audience, request.WritingStyle, cancellationToken);
        return Ok(ToResponse(blogEntry));
    }

    /// <summary>
    /// Start blog generation as a background job. Subscribe to
    /// /api/agent/blog/{jobId}/stream for real-time progress, then poll
    /// this endpoint's result via GET result/{jobId} once complete.
    /// </summary>
    [HttpPost("CreateBlogEntryAsync")]
    public async Task<IActionResult> CreateBlogEntryAsync([FromBody] CreateBlogRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Topic))
            return BadRequest("Topic is required.");

        var jobId = await blogService.StartBlogEntryJobAsync(request.Topic, request.Audience, request.WritingStyle, cancellationToken);
        return Accepted(new BlogJobStartedResponse { JobId = jobId });
    }

    /// <summary>
    /// Get the result of a blog generation job started via CreateBlogEntryAsync.
    /// </summary>
    [HttpGet("result/{jobId}")]
    public IActionResult GetJobResult(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return BadRequest("jobId is required.");

        var result = blogService.GetJobResult(jobId);
        if (result == null)
            return Accepted(new { jobId, status = "processing" });

        if (!result.IsSuccess)
            return StatusCode(StatusCodes.Status500InternalServerError, new { jobId, error = result.Error });

        return Ok(ToResponse(result.Entry!));
    }

    private static BlogEntryResponse ToResponse(BlogEntry blogEntry) => new()
    {
        Content = blogEntry.Content,
        Citations = blogEntry.Citations,
        References = blogEntry.References,
        QualityScore = blogEntry.QualityAssessment?.QualityScore ?? 0,
        QualityIssues = blogEntry.QualityAssessment?.IssuesFound ?? new(),
        QualityStrengths = blogEntry.QualityAssessment?.Strengths ?? new()
    };
}
