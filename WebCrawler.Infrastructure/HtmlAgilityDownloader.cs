using HtmlAgilityPack;
using WebCrawler.Domain.Ports;

namespace WebCrawler.Infrastructure
{
    public class HtmlAgilityDownloader : IHtmlDownloader
    {
        public async Task<string> GetHtmlContentAsync(string url)
        {
            return await Task.Run(() =>
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);
                return doc.DocumentNode.OuterHtml;
            });
        }
    }
}
