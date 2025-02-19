using WebCrawler.Domain.Models;

namespace WebCrawler.Domain.Ports
{
    public interface IProxyRepository
    {
        Task<int> SaveExecutionAsync(CrawlerExecution execution);
    }
}
