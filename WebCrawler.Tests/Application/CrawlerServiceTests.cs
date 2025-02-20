using Moq;
using WebCrawler.Domain.Models;
using WebCrawler.Domain.Ports;
using WebCrawler.Application;
using WebCrawler.Application.DTOs;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebCrawler.Tests
{
    public class CrawlerServiceTests
    {
        [Fact]
        public async Task RunCrawlerAsync_ShouldStopAtPage4_WhenPage4IsEmpty()
        {
            // Arrange

            // 1) Mock do IHtmlDownloader
            var downloaderMock = new Mock<IHtmlDownloader>();
            // Sempre devolve um HTML "fake"
            downloaderMock
                .Setup(d => d.GetHtmlContentAsync(It.IsAny<string>()))
                .ReturnsAsync("<html>fake</html>");

            // 2) Preparamos um dicionário page->proxies
            var pageData = new Dictionary<int, List<ProxyInfo>> {
                {1, CreateProxies(5, "page1")}, // 5 proxies
                {2, CreateProxies(4, "page2")},
                {3, CreateProxies(3, "page3")},
                {4, new List<ProxyInfo>()}      // vazio => deve parar
            };

            // 3) Mock do IProxyParser
            var parserMock = new Mock<IProxyParser>();
            // Sempre que chamar ParseProxies, decidimos a lista pelo pageNumber
            parserMock
                .Setup(p => p.ParseProxies(It.IsAny<string>()))
                .Returns((string html) =>
                {
                    // Supondo que você de alguma forma extrai pageNumber do HTML
                    // ou a URL (ex.: page/1) - mas aqui faremos algo simplificado:
                    // Vamos simular um contador estático ou algo do tipo
                    // MAS melhor extrair "pageNumber" do "html" se tiver um snippet.
                    // Aqui, iremos fingir que chamamos "page1", "page2", ...
                    // Precisamos de um modo de saber a "atual".
                    // Se o seu Crawler a cada loop cria "<html>page1</html>" no downloader,
                    // você pode extrair a substring.

                    // Para didático, assumimos que chamará em ordem 1,2,3,4.
                    // Se for simultâneo, a ordem pode variar, mas iremos "drivar" por
                    // quantas vezes ParseProxies já foi chamada. Isso não reflete bem a
                    // concurrency. De todo modo, segue exemplo "didático":

                    int calls = _callCounter++;
                    switch (calls)
                    {
                        case 0: return pageData[1];
                        case 1: return pageData[2];
                        case 2: return pageData[3];
                        default: return pageData[4];
                    }
                });

            // 4) Mock do IPagePrinter
            var printerMock = new Mock<IPagePrinter>();
            // Nao precisamos ver retorno, mas podemos verificar se foi chamado



            var loggerMock = new Mock<ILogger<CrawlerService>>();


            // 5) Instanciar CrawlerService
            var crawlerService = new CrawlerService(
                downloaderMock.Object,
                parserMock.Object,
                printerMock.Object,
                loggerMock.Object
            );

            // Act
            var result = await crawlerService.RunCrawlerAsync();

            // Assert
            // Esperamos 5+4+3 = 12 proxies e parar ao chegar em page4 vazia
            Assert.Equal(12, result.Proxies.Count);
            Assert.Equal(3, result.PagesCount); // so pag1, pag2, pag3 tiveram proxies

            // Verifica se page4 realmente foi processada (1 chamou parseProxies 4 vezes)
            // e se page5 nunca foi enfileirada => entao parseProxies nao deve ter sido
            // chamada 5x. 
            // Se "calls" passou de 4, ja eh suspeito.

            // Podemos tbm checar se PrintPageAsync foi chamado 4 vezes no total
            printerMock.Verify(p => p.PrintPageAsync(It.IsAny<string>(), It.IsAny<int>()),
                               Times.Exactly(4));
        }

        // Contador estático pra sequenciar as chamadas do parser
        private static int _callCounter = 0;

        private List<ProxyInfo> CreateProxies(int qtd, string marker)
        {
            var list = new List<ProxyInfo>();
            for (int i = 1; i <= qtd; i++)
            {
                list.Add(new ProxyInfo($"IP{i}-{marker}",
                                       i.ToString(),
                                       "Country",
                                       "HTTP"));
            }
            return list;
        }
    }
}
