using HtmlAgilityPack;
using WebCrawler.Domain.Models;
using WebCrawler.Domain.Ports;

namespace WebCrawler.Infrastructure
{
    public class HtmlAgilityProxyParser : IProxyParser
    {
        public List<ProxyInfo> ParseProxies(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Seleciona os <tr> conforme o site
            var rowNodes = doc.DocumentNode.SelectNodes("//tbody/tr");
            if (rowNodes == null)
                return new List<ProxyInfo>();

            var proxies = new List<ProxyInfo>();

            foreach (var row in rowNodes)
            {
                var cells = row.SelectNodes("td");
                if (cells == null || cells.Count < 7)
                    continue;

                // 2a <td> => IP Address
                string ipAddress = cells[1].InnerText.Trim();

                // 3a <td> => Port (hex em data-port)
                var portSpan = cells[2].SelectSingleNode(".//span[@class='port']");
                string? portHex = portSpan?.GetAttributeValue("data-port", null);

                // 4a <td> => Country
                string country = cells[3].InnerText.Trim();

                // 7a <td> => Protocol
                string protocol = cells[6].InnerText.Trim();

                var proxy = new ProxyInfo(ipAddress, portHex, country, protocol);
                proxies.Add(proxy);
            }

            return proxies;
        }
    }
}
