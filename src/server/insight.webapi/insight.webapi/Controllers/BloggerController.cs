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

        var content = await blogService.CreateBlogEntryAsync(request.Topic, request.Audience, request.WritingStyle, cancellationToken);
        return Ok(new BlogEntryResponse { Content = content });
    }
}
