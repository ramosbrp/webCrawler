
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
            // 1. Baixar conteúdo HTML
            var html = await GetHtmlContentAsync(url);

            return new List<ProxyInfo>();
        }

        private async Task<string> GetHtmlContentAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

    }
}
