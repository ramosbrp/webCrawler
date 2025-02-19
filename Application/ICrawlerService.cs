using WebCrawler.Domain.Models;

namespace WebCrawler.Application
{
    public interface ICrawlerService
    {
        Task<List<ProxyInfo>> RunCrawlerAsync();
    }

}

