using WebCrawler.Domain.Ports;

namespace WebCrawler.Infrastructure
{
    public class FilePagePrinter : IPagePrinter
    {
        public async Task PrintPageAsync(string html, int pageNumber)
        {
            // Defina a pasta
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "PagesHtml");
            Directory.CreateDirectory(folderPath);

            // Monta o nome do arquivo: page_1.html, page_2.html, etc.
            string fileName = $"page_{pageNumber}_{DateTime.Now:yyyyMMddHHmmss}.html";
            string filePath = Path.Combine(folderPath, fileName);

            // Salva o conteúdo HTML no arquivo
            await File.WriteAllTextAsync(filePath, html);
        }
    }
}
