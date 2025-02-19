namespace WebCrawler.Domain.Ports
{
    public interface IHtmlDownloader
    {
        Task<string> GetHtmlContentAsync(string url);
    }
}
