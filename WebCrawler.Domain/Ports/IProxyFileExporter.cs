using WebCrawler.Domain.Models;

namespace WebCrawler.Domain.Ports
{
    public interface IProxyFileExporter
    {
        Task<string> SaveProxiesToJsonAsync(IEnumerable<ProxyInfo> proxies);
    }
}
