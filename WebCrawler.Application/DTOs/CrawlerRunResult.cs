using WebCrawler.Domain.Models;

namespace WebCrawler.Application.DTOs
{
    public class CrawlerRunResult
    {
        public List<ProxyInfo> Proxies { get; set; }
        public int PagesCount { get; set; }

        public CrawlerRunResult(List<ProxyInfo> proxies, int pagesCount)
        {
            Proxies = proxies;
            PagesCount = pagesCount;
        }
    }
}
