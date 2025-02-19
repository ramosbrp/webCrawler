using System.Text.Json;
using WebCrawler.Domain.Models;
using WebCrawler.Domain.Ports;

namespace WebCrawler.Infrastructure
{
    public class JsonProxyFileExporter : IProxyFileExporter
    {
        public async Task<string> SaveProxiesToJsonAsync(IEnumerable<ProxyInfo> proxies)
        {
            var outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "Output");
            Directory.CreateDirectory(outputFolder);

            var filePath = Path.Combine(outputFolder, $"proxies-{DateTime.Now:yyyyMMddHHmmss}.json");

            var json = JsonSerializer.Serialize(proxies, new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(filePath, json);

            return filePath;
        }
    }
}
