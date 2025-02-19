using HtmlAgilityPack;
using WebCrawler.Domain.Models;

namespace WebCrawler.Services
{
    public class CrawlerService
    {

        private readonly HttpClient _httpClient;

        public CrawlerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProxyInfo>> ExtractProxiesFromPageAsync(string url)
        {

            var rowNodes = await GetHtmlContentAsync(url);

            // Se não encontrou rows, devolve lista vazia
            if (rowNodes == null)
                return new List<ProxyInfo>();

            var proxies = new List<ProxyInfo>();

            // Para cada <tr>, vamos extrair as <td>
            foreach (var row in rowNodes)
            {
                // Pega todos os <td> (células) desse <tr>
                var cells = row.SelectNodes("td");
                if (cells == null)
                    continue; // sem células, pula

                // (HTML, a 2a <td> é IP Address, 3a <td> é Port, 4a <td> é Country e 7a <td> é Protocol, por exemplo.)

                // IP Address (Na 2a TD há uma <a> ou text)
                string ipAddress = cells[1].InnerText.Trim();

                // Então vamos buscar esse span ou pegar o "data-port":
                var portSpan = cells[2].SelectSingleNode(".//span[@class='port']");

                // "data-port" contém valor em hexa, ex: "0B07"
                string? port = portSpan?.GetAttributeValue("data-port", null);
                // int portDecimal = Convert.ToInt32(portHex, 16);

                // cells[3].InnerText
                string country = cells[3].InnerText.Trim();

                // Protocol (no seu HTML, a 7a <td>)
                string protocol = cells[6].InnerText.Trim();

                // Monta o objeto
                var proxy = new ProxyInfo(ipAddress, port, country, protocol);

                proxies.Add(proxy);
            }

            return proxies;
        }


        public async Task<List<ProxyInfo>> RunAsync()
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

        private async Task<HtmlNodeCollection> GetHtmlContentAsync(string url)
        {
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = await Task.Run(() => web.Load(url));
            var rowNodes = htmlDoc.DocumentNode.SelectNodes("//tbody/tr");
            return rowNodes;
        }

    }
}
