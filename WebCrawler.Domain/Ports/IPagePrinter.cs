namespace WebCrawler.Domain.Ports
{
    public interface IPagePrinter
    {
        Task PrintPageAsync(string html, int pageNumber);
    }
}
