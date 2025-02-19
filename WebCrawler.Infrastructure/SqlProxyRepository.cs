using WebCrawler.Domain.Ports;
using System.Data.SqlClient;
//using Dapper;
using System.Threading.Tasks;
using WebCrawler.Domain.Ports;
using WebCrawler.Domain.Models;

namespace WebCrawler.Infrastructure
{
    public class SqlProxyRepository : IProxyRepository
    {
        private readonly string _connectionString;

        public SqlProxyRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> SaveExecutionAsync(CrawlerExecution execution)
        {
            //using var conn = new SqlConnection(_connectionString);

            //string sql = @"
            //    INSERT INTO CrawlerExecutions
            //    (StartTime, EndTime, PagesCount, TotalRecords, JsonFilePath)
            //    VALUES (@StartTime, @EndTime, @PagesCount, @TotalRecords, @JsonFilePath);
            //    SELECT CAST(SCOPE_IDENTITY() AS int);
            //";

            //var newId = await conn.ExecuteScalarAsync<int>(sql, execution);
            return 1;
        }
    }
}
