using WebCrawler.Domain.Models;
using WebCrawler.Domain.Ports;

namespace WebCrawler.Application
{
    public class CrawlerService : ICrawlerService
    {
        private readonly IHtmlDownloader _htmlDownloader;
        private readonly IProxyParser _proxyParser;

        public CrawlerService(IHtmlDownloader htmlDownloader, IProxyParser proxyParser)
        {
            _htmlDownloader = htmlDownloader;
            _proxyParser = proxyParser;
        }

        public async Task<List<ProxyInfo>> RunCrawlerAsync()
        {
            try
            {
                var allProxies = new List<ProxyInfo>();
                var pageNumber = 1;
                bool hasMorePages = true;

                while (hasMorePages)
                {
                    var url = $"https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/{pageNumber}";
                    Console.WriteLine($"Processando URL: {url}");

                    // 1. Tenta extrair
                    var html = await _htmlDownloader.GetHtmlContentAsync(url);

                    // 2. Faz parse dos proxies
                    var proxies = _proxyParser.ParseProxies(html);

                    if (proxies.Count != 0)
                    {
                        allProxies.AddRange(proxies);
                        pageNumber++;
                    }
                    else
                    {
                        // Se não retornou nada, pode indicar que chegamos ao fim
                        hasMorePages = false;
                    }
                }

                return allProxies;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


    }
}
