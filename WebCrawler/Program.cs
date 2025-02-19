using WebCrawler.Services;

// 1. Criar HttpClient
var httpClient = new HttpClient();

// 2. Instanciar Service
var crawlerService = new CrawlerService(httpClient);

var proxies = await crawlerService.RunAsync();