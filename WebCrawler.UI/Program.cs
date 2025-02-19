using WebCrawler.Application;

// 1. Criar HttpClient
var httpClient = new HttpClient();

// 2. Instanciar Service
var crawlerService = new CrawlerService(httpClient);


// 3. Guardar StartTime
var startTime = DateTime.Now;
Console.WriteLine($"Execução iniciada em {startTime}");

// 4. Executar processo
var proxies = await crawlerService.RunAsync();

// 5. Salvar JSON
