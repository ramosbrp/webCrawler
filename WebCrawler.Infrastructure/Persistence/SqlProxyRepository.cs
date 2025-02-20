using WebCrawler.Domain.Ports;
using WebCrawler.Domain.Models;

namespace WebCrawler.Infrastructure.Persistence
{
    public class SqlProxyRepository : IProxyRepository
    {
        private readonly CrawlerDbContext _context;
        public SqlProxyRepository(CrawlerDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveExecutionAsync(CrawlerExecution execution)
        {
            try
            {
                _context.CrawlerExecutions.Add(execution);
                await _context.SaveChangesAsync();
                return 1;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}
