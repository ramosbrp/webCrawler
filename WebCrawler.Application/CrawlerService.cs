using WebCrawler.Application.DTOs;
using WebCrawler.Domain.Models;
using WebCrawler.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace WebCrawler.Application
{
    public class CrawlerService : ICrawlerService
    {
        private readonly IHtmlDownloader _htmlDownloader;
        private readonly IProxyParser _proxyParser;
        private readonly IPagePrinter _pagePrinter;
        private readonly ILogger<CrawlerService> _logger;

        public CrawlerService(
            IHtmlDownloader htmlDownloader,
            IProxyParser proxyParser,
            IPagePrinter pagePrinter,
            ILogger<CrawlerService> logger)
        {
            _htmlDownloader = htmlDownloader;
            _proxyParser = proxyParser;
            _pagePrinter = pagePrinter;
            _logger = logger;
        }

        public async Task<CrawlerRunResult> RunCrawlerAsync()
        {
            try
            {
                var allProxies = new List<ProxyInfo>();
                var pageNumber = 1;
                bool hasMorePages = true;
                int pagesProcessed = 0;

                while (hasMorePages)
                {
                    var url = $"https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/{pageNumber}";
                    Console.WriteLine($"Processando URL: {url}");

                    // 1. Baixar HTML
                    var html = await _htmlDownloader.GetHtmlContentAsync(url);

                    // 2. Salvar (print) em arquivo
                    await _pagePrinter.PrintPageAsync(html, pageNumber);

                    // 3. Parsear proxies
                    var proxies = _proxyParser.ParseProxies(html);

                    if (proxies.Count != 0)
                    {
                        allProxies.AddRange(proxies);
                        pageNumber++;
                        pagesProcessed++;
                    }
                    else
                    {
                        // Se não retornou nada, pode indicar que chegamos ao fim
                        hasMorePages = false;
                    }
                }

                return new CrawlerRunResult(allProxies, pagesProcessed);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante RunCrawlerAsync no pageNumber X");
                throw;
            }
        }
    }
}
