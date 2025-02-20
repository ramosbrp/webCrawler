using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawler.Domain.Models;

namespace WebCrawler.Infrastructure.Persistence
{
    public class CrawlerDbContext : DbContext
    {
        public CrawlerDbContext(DbContextOptions<CrawlerDbContext> options)
            : base(options)
        {
        }

        // DbSets (tabelas) que iremos mapear
        public DbSet<CrawlerExecution> CrawlerExecutions { get; set; }
        public DbSet<ProxyInfo> Proxies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar a tabela
            modelBuilder.Entity<CrawlerExecution>(entity =>
            {
                entity.ToTable("CrawlerExecutions"); 
                entity.HasKey(e => e.Id);
            });

            //modelBuilder.Entity<ProxyInfo>(entity =>
            //{
            //    entity.ToTable("Proxies");
            //    entity.HasKey(p => p.Id);
            //});
        }
    }
}
