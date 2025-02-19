using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using WebCrawler.Models;

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

        private async Task<HtmlNodeCollection> GetHtmlContentAsync(string url)
        {
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = await Task.Run(() => web.Load(url));
            var rowNodes = htmlDoc.DocumentNode.SelectNodes("//tbody/tr");
            return rowNodes;
        }

    }
}
