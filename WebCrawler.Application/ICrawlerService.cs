using WebCrawler.Application.DTOs;
using WebCrawler.Domain.Models;

namespace WebCrawler.Application
{
    public interface ICrawlerService
    {
        Task<CrawlerRunResult> RunCrawlerAsync();
    }

}

