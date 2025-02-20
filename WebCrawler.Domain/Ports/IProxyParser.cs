using WebCrawler.Domain.Models;

namespace WebCrawler.Domain.Ports
{
    public interface IProxyParser
    {
        List<ProxyInfo> ParseProxies(string html);
        List<ProxyInfo> ParseTeste(string html);
        
    }
}
