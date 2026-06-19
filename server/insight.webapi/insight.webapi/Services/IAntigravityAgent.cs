using System.Threading;
using System.Threading.Tasks;

namespace insight.webapi.Services
{
    public interface IAntigravityAgent
    {
        Task<string> CreateBlogPostAsync(string topic, CancellationToken cancellationToken = default);
    }
}
