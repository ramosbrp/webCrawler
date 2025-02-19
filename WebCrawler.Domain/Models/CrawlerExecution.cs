namespace WebCrawler.Domain.Models
{
    public class CrawlerExecution
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int PagesCount { get; set; }
        public int TotalRecords { get; set; }
        public string JsonFilePath { get; set; }
    }
}
