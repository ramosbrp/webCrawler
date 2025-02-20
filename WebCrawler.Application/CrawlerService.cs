using WebCrawler.Application.DTOs;
using WebCrawler.Domain.Models;
using WebCrawler.Domain.Ports;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace WebCrawler.Application
{
    public class CrawlerService : ICrawlerService
    {
        private readonly IHtmlDownloader _htmlDownloader;
        private readonly IProxyParser _proxyParser;
        private readonly IPagePrinter _pagePrinter;
        private readonly ILogger<CrawlerService> _logger;

        // Sinal global de “pare tudo”
        private bool _stopAll = false;

        // Fila concorrente de páginas a processar
        private ConcurrentQueue<int> _pagesQueue = new ConcurrentQueue<int>();

        // Contador de quantas páginas realmente tiveram proxies
        private int _pagesProcessed = 0;

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
            return await RunCrawlerInParallelAsync();
        }

        private async Task<CrawlerRunResult> RunCrawlerInParallelAsync()
        {
            try
            {

                // 1) Enfileira a primeira página
                _pagesQueue.Enqueue(1);

                // 2) Prepara bag para colecionar proxies
                var allProxies = new ConcurrentBag<ProxyInfo>();

                // 3) Configura quantos “workers” queremos
                const int WORKERS = 3;
                var tasks = new List<Task>(WORKERS);

                for (int i = 0; i < WORKERS; i++)
                {
                    tasks.Add(Task.Run(() => WorkerAsync(allProxies)));
                }

                // 4) Aguarda todos os workers terminarem
                await Task.WhenAll(tasks);

                // 5) Monta o resultado
                //    Suponha que a quantidade de páginas é o total processado
                //    ou algum contador. Aqui, para simplificar, passamos 0.
                return new CrawlerRunResult(allProxies.ToList(), _pagesProcessed);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante RunCrawlerAsync no pageNumber X");
                throw;
            }
        }

        private async Task WorkerAsync(ConcurrentBag<ProxyInfo> allProxies)
        {
            while (!_stopAll)
            {
                // Tenta pegar uma página da fila
                if (_pagesQueue.TryDequeue(out int pageNumber))
                {
                    // Monta a URL
                    var url = $"https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/{pageNumber}";

                    // Baixa o HTML
                    var html = await _htmlDownloader.GetHtmlContentAsync(url);
                    Console.WriteLine($"Thread {Task.CurrentId} processando URL: {url}");

                    // Salva (“print”) em arquivo se quiser
                    await _pagePrinter.PrintPageAsync(html, pageNumber);

                    // Faz parse dos proxies
                    var proxies = _proxyParser.ParseProxies(html);

                    if (proxies.Count == 0)
                    {
                        // Se estiver vazio, definimos _stopAll para
                        // parar os demais workers gradualmente.
                        _stopAll = true;
                    }
                    else
                    {
                        // Adiciona proxies no bag
                        foreach (var p in proxies)
                            allProxies.Add(p);

                        // Incrementa contagem de páginas processadas
                        // (usar Interlocked para thread safety)
                        Interlocked.Increment(ref _pagesProcessed);

                        // Enfileira a PRÓXIMA página
                        _pagesQueue.Enqueue(pageNumber + 1);

                    }

                }
                else
                {
                    // Se não há nenhuma página na fila no momento, espere um pouco
                    // para não ficar em loop consumindo CPU.
                    await Task.Delay(100);
                }
            }
        }
    }
}
