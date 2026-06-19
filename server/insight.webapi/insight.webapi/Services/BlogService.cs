using System.Threading;
using System.Threading.Tasks;

namespace insight.webapi.Services
{
    public class BlogService : IBlogService
    {
        private readonly IAntigravityAgent _agent;

        public BlogService(IAntigravityAgent agent) => _agent = agent;

        public async Task<string> CreateBlogEntryAsync(string topic, CancellationToken cancellationToken = default)
        {
            // place for validation, persistence, metrics, etc.
            return await _agent.CreateBlogPostAsync(topic, cancellationToken);
        }
    }
}
