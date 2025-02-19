using WebCrawler.Domain.Models;
using WebCrawler.Domain.Ports;

namespace WebCrawler.Application
{
    public class CrawlerService : ICrawlerService
    {
        private readonly IHtmlDownloader _htmlDownloader;

        public CrawlerService(IHtmlDownloader htmlDownloader)
        {
            _htmlDownloader = htmlDownloader;
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

                    // Tenta extrair
                    var proxies = await ExtractProxiesFromPageAsync(url);

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

        private async Task<List<ProxyInfo>> ExtractProxiesFromPageAsync(string url)
        {

            var rowNodes = await _htmlDownloader.GetHtmlContentAsync(url);

            // Se não encontrou rows, devolve lista vazia
            if (rowNodes == null)
                return new List<ProxyInfo>();

            var proxies = new List<ProxyInfo>();

            

            return proxies;
        }

    }
}
