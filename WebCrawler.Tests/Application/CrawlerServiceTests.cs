using Moq;
using WebCrawler.Application;
using WebCrawler.Application.DTOs;
using WebCrawler.Domain.Ports;
using WebCrawler.Domain.Models;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace WebCrawler.Tests
{
    public class CrawlerServiceTests
    {
        [Fact]
        public async Task RunCrawlerAsync_ShouldStop_WhenParserReturnsNoProxies()
        {
            // Arrange

            // 1) Mock do IHtmlDownloader (sempre devolve "HTML fake")
            var downloaderMock = new Mock<IHtmlDownloader>();
            downloaderMock
                .Setup(d => d.GetHtmlContentAsync(It.IsAny<string>()))
                .ReturnsAsync("<html>fake</html>");

            // 2) Mock do IProxyParser
            // No primeiro page=1 retorna 1 proxy; 
            // No page=2 retorna 0 proxies => deve parar
            var parserMock = new Mock<IProxyParser>();
            parserMock
                .SetupSequence(p => p.ParseProxies(It.IsAny<string>()))
                .Returns(new List<ProxyInfo> { new ProxyInfo("1.2.3.4", "8080", "Brasil", "HTTP") })
                .Returns(new List<ProxyInfo>()); // 2a chamada => vazio

            // 3) Mock do IPagePrinter (não precisamos de retorno, só verificar se foi chamado)
            var printerMock = new Mock<IPagePrinter>();
            var loggerMock = new Mock<ILogger<CrawlerService>>();

            // 4) Instanciamos o CrawlerService com Mocks
            var service = new CrawlerService(
                downloaderMock.Object,
                parserMock.Object,
                printerMock.Object,
                loggerMock.Object
            );

            // Act
            var result = await service.RunCrawlerAsync();

            // Assert
            // Se foi apenas a page=1 que deu proxies, o total deve ser 1
            Assert.Single(result.Proxies);
            // Se o crawler parou na página 2 (vazia), result.PagesCount deve ser 1 (nessa nossa abordagem).
            Assert.Equal(1, result.PagesCount);

            // Verificamos se a 2a chamada veio vazia
            parserMock.Verify(p => p.ParseProxies("<html>fake</html>"), Times.Exactly(2));
            // Verifica se PrintPage foi chamado 2 vezes
            printerMock.Verify(p => p.PrintPageAsync("<html>fake</html>", It.IsAny<int>()), Times.Exactly(2));
        }
    }
}
