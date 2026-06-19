using System.Threading;
using System.Threading.Tasks;

namespace insight.webapi.Services
{
    public interface IBlogService
    {
        Task<string> CreateBlogEntryAsync(string topic, CancellationToken cancellationToken = default);
    }
}
