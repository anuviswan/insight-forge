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
        return Ok(new BlogEntryResponse
        {
            Content = blogEntry.Content,
            Citations = blogEntry.Citations,
            References = blogEntry.References,
            QualityScore = blogEntry.QualityAssessment?.QualityScore ?? 0,
            QualityIssues = blogEntry.QualityAssessment?.IssuesFound ?? new(),
            QualityStrengths = blogEntry.QualityAssessment?.Strengths ?? new()
        });
    }
}
