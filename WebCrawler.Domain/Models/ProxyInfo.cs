namespace WebCrawler.Domain.Models
{
    public class ProxyInfo(string ipAddress, string? port, string country, string protocol)
    {
        //public int Id { get; set; } = 0;
        public string IpAddress { get; set; } = ipAddress;
        public string? Port { get; set; } = port;
        public string Country { get; set; } = country;
        public string Protocol { get; set; } = protocol;
    }
}
