namespace WebCrawler.Domain.Ports
{
    public interface IProxyRepository
    {
        Task<int> SaveExecutionAsync();
    }
}
