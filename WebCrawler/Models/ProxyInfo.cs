namespace WebCrawler.Models
{
    public class ProxyInfo
    {
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public string Country { get; set; }
        public string Protocol { get; set; }

        public ProxyInfo(string ipAddress, string port, string country, string protocol)
        {
            IpAddress = ipAddress;
            Port = port;
            Country = country;
            Protocol = protocol;
        }
    }
}
