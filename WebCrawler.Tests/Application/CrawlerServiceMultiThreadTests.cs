using Moq;
using WebCrawler.Domain.Ports;
using WebCrawler.Domain.Models;
using WebCrawler.Application;
using Microsoft.Extensions.Logging;

// Importe também seu "CrawlerService" ou a classe exata que implementa a fila + 3 workers
// e as interfaces IHtmlDownloader, IProxyParser, IPagePrinter, etc.

namespace WebCrawler.Tests
{
    public class CrawlerServiceMultiThreadTests
    {
        [Fact]
        public async Task RunCrawlerAsync_MultiThread_TestPages123_Then4Empty_ShouldStopAndCollectAllProxies()
        {
            // 1. ARRANGE

            // a) Criamos mocks para IHtmlDownloader, IProxyParser, IPagePrinter
            var downloaderMock = new Mock<IHtmlDownloader>();
            var parserMock = new Mock<IProxyParser>();
            var printerMock = new Mock<IPagePrinter>();

            // b) Configuramos o downloaderMock para sempre devolver um HTML "fake"
            //    O parse real será ditado pelo parserMock. Se quiser, podemos distinguir 
            //    HTML por pageNumber, mas normalmente iremos diferenciar pelo parserMock.
            downloaderMock
                .Setup(d => d.GetHtmlContentAsync(It.IsAny<string>()))
                .ReturnsAsync("<html>FAKE</html>");

            // c) Precisamos distinguir quantos proxies cada "página" retorna
            //    Um dicionário para mapear: pageNumber -> List<ProxyInfo>
            var pageData = new Dictionary<int, List<ProxyInfo>> {
                { 1, CreateProxies(5, "page1") },  // 5 proxies
                { 2, CreateProxies(4, "page2") },  // 4 proxies
                { 3, CreateProxies(3, "page3") },  // 3 proxies
                { 4, new List<ProxyInfo>()        } // vazio => deve parar
            };

            // d) Configuramos o parser para, sempre que chamado, descobrir pageNumber na URL
            //    e retornar a lista do dicionário. Se for >=4 e não tiver no dicionário, retorna vazio.
            parserMock
                .Setup(p => p.ParseProxies(It.IsAny<string>()))
                .Returns((string html) =>
                {
                    // Precisamos descobrir qual pageNumber está sendo processada
                    // por ex: ".../page/1". Vamos parsear da string ou 
                    // podemos armazenar "pageNumber" em chamador. Simularemos de modo simples aqui:
                    // Se você tiver acesso à URL, normalmente moquearíamos de outra forma
                    // MAS se o CrawlerService parseia a string, faça de outro jeito.
                    // Para simplificar, assumimos que o CrawlerService extrai "pageNumber" de sua fila.
                    // Vamos fingir que "Thread.CurrentId" ou algo do tipo, mas isso não é confiável.

                    // Abordagem: em vez disso, se o CrawlerService sempre formata a URL 
                    // ".../page/{pageNumber}", podemos interceptar a call do downloader 
                    // e armazenar o pageNumber. 
                    // Mas aqui faremos algo "didático" e forçado.

                    // OPÇÃO: iremos supor que o teste roda em sequência: 1, 2, 3, 4
                    // e "contaremos" quantas vezes a parseProxies foi chamada
                    // Sequencial mock: 1->5 proxies, 2->4, 3->3, 4->0

                    // MAS no multiThread, a ordem de chamadas pode ser {1,2,3} meio aleatória.
                    // Precisaremos interceptar a URL de getHtmlContentAsync para ver pageNumber real.

                    // Então é melhor "Interceptar" a URL no downloaderMock e 
                    // associar a um callback local. Vamos mostrar como a seguir.
                    return new List<ProxyInfo>(); // voltaremos a esse ponto
                });

            // e) Precisamos interceptar a URL exata no downloader:
            //    Ex: "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/1"
            //    e extrair o "1" no final, para sabermos qual "pageNumber" estamos simulando.

            downloaderMock
                .Setup(d => d.GetHtmlContentAsync(It.IsAny<string>()))
                .Returns<string>(url =>
                {
                    // Extraímos a parte final do URL
                    // Ex: ".../page/2" => "2"
                    var pageNumber = ExtractPageNumber(url);

                    // Retornamos HTML fake
                    return Task.FromResult($"<html>FAKE for page {pageNumber}</html>");
                });

            // Agora ajustamos o parserMock para retornar base no pageNumber
            parserMock
                .Setup(p => p.ParseProxies(It.Is<string>(s => s.Contains("page"))))
                .Returns((string html) =>
                {
                    // descobre pageNumber do html
                    var pageNum = ExtractPageNumberFromHtml(html);
                    // pega do dicionario
                    if (pageData.TryGetValue(pageNum, out var proxies))
                        return proxies;
                    else
                        return new List<ProxyInfo>();
                });

            var loggerMock = new Mock<ILogger<CrawlerService>>();

            // f) Montamos o CrawlerService com a abordagem "fila e 3 workers"
            var crawlerService = new CrawlerService(
                downloaderMock.Object,
                parserMock.Object,
                printerMock.Object,
                loggerMock.Object
            );

            // 2. ACT
            var result = await crawlerService.RunCrawlerAsync();

            // 3. ASSERT

            // Queremos 5 + 4 + 3 = 12 proxies no total
            Assert.Equal(12, result.Proxies.Count);

            // Se a página 4 está vazia, paramos, logo "PagesCount" deve ser 3
            Assert.Equal(3, result.PagesCount);

            // Verificar se foi chamado PrintPageAsync para as páginas 1,2,3,4
            // 4 será chamada mas vazia => dispara a flag _stopAll
            printerMock.Verify(
                p => p.PrintPageAsync(It.IsAny<string>(), 1),
                Times.AtLeastOnce
            );
            printerMock.Verify(
                p => p.PrintPageAsync(It.IsAny<string>(), 2),
                Times.AtLeastOnce
            );
            printerMock.Verify(
                p => p.PrintPageAsync(It.IsAny<string>(), 3),
                Times.AtLeastOnce
            );
            printerMock.Verify(
                p => p.PrintPageAsync(It.IsAny<string>(), 4),
                Times.AtLeastOnce
            );

            // Se quiser, podemos ver quantas vezes "page 5" foi chamada => zero vezes
            printerMock.Verify(
                p => p.PrintPageAsync(It.IsAny<string>(), 5),
                Times.Never
            );
        }

        // Cria proxies fakes
        private List<ProxyInfo> CreateProxies(int qtd, string marker)
        {
            var list = new List<ProxyInfo>();
            for (int i = 1; i <= qtd; i++)
            {
                list.Add(new ProxyInfo(
                    $"IP{i}-{marker}",
                    i.ToString(),
                    "Country",
                    "HTTP"
                ));
            }
            return list;
        }

        // Extrai pageNumber do URL
        private int ExtractPageNumber(string url)
        {
            // Ex:  "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/3"
            // Pega a substring após "/page/"
            var idx = url.IndexOf("/page/");
            if (idx < 0) return 1; // fallback
            var pageStr = url[(idx + 6)..]; // 6 = length of "/page/"
            if (int.TryParse(pageStr, out int pageNum))
                return pageNum;
            return 1;
        }

        // Extrai pageNumber do HTML, ex.: "<html>FAKE for page 2</html>"
        private int ExtractPageNumberFromHtml(string html)
        {
            // Ex: "FAKE for page 2"
            var idx = html.IndexOf("page ");
            if (idx < 0) return 1;
            var substring = html[(idx + 5)..];
            if (int.TryParse(substring, out int pageNum))
                return pageNum;
            return 1;
        }
    }
}
