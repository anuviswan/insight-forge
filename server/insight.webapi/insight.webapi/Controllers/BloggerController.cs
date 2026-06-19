using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using insight.webapi.Models;
using insight.webapi.Services;

namespace insight.webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BloggerController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BloggerController(IBlogService blogService) => _blogService = blogService;

        [HttpPost("CreateBlogEntry")]
        public async Task<IActionResult> CreateBlogEntry([FromBody] CreateBlogRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Topic))
                return BadRequest("Topic is required.");

            var content = await _blogService.CreateBlogEntryAsync(request.Topic, cancellationToken);
            return Ok(new BlogEntryResponse { Content = content });
        }
    }
}
