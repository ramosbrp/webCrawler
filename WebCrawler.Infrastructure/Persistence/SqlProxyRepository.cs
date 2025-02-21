using WebCrawler.Domain.Ports;
using WebCrawler.Domain.Models;
using Microsoft.Extensions.Logging;

namespace WebCrawler.Infrastructure.Persistence
{
    public class SqlProxyRepository : IProxyRepository
    {
        private readonly CrawlerDbContext _context;
        private readonly ILogger<SqlProxyRepository> _logger;
        public SqlProxyRepository(CrawlerDbContext context, ILogger<SqlProxyRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> SaveExecutionAsync(CrawlerExecution execution)
        {
            try
            {
                _context.CrawlerExecutions.Add(execution);
                await _context.SaveChangesAsync();
                return execution.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar execução no banco");
                // Se não houver ação de recuperação, re-lancar a exceção
                throw;
            }

        }
    }
}
