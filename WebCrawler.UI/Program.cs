using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebCrawler.Application;
using WebCrawler.Domain.Ports;
using WebCrawler.Infrastructure;




// Cria um Host que gerencia DI
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Adicionar implementações concretas para interfaces
        services.AddTransient<IHtmlDownloader, HtmlAgilityDownloader>();
        services.AddTransient<ICrawlerService, CrawlerService>();
        services.AddTransient<IProxyFileExporter, JsonProxyFileExporter>();
        services.AddTransient<IProxyParser, HtmlAgilityProxyParser>();

        // Precisamos passar a connection string do config:
        var connectionString = "Server=...;Database=...;User Id=...;Password=...";
        services.AddTransient<IProxyRepository>(provider =>
            new SqlProxyRepository(connectionString));
    })
    .Build();


var crawlerService = host.Services.GetRequiredService<ICrawlerService>();
var exporter = host.Services.GetRequiredService<IProxyFileExporter>();
var repo = host.Services.GetRequiredService<IProxyRepository>();


// Início da execução
var startTime = DateTime.Now;
Console.WriteLine($"Iniciando crawler em {startTime}");

// Executa
Console.WriteLine($"Obtendo proxies - {DateTime.Now}");
var proxies = await crawlerService.RunCrawlerAsync();

// Salva em JSON
Console.WriteLine($"Salvando proxies obtidos em Json - {DateTime.Now}");
var filePath = await exporter.SaveProxiesToJsonAsync(proxies);

var endTime = DateTime.Now;

// Salva log de execução no banco
var exec = new WebCrawler.Domain.Models.CrawlerExecution
{
    StartTime = startTime,
    EndTime = endTime,
    PagesCount = 1, // Ajustar para a contagem real 
    TotalRecords = proxies.Count,
    JsonFilePath = filePath
};

Console.WriteLine($"Salvando log da execução no banco de dados - {DateTime.Now}");
var newId = await repo.SaveExecutionAsync(exec);

Console.WriteLine($"Execução finalizada. {proxies.Count} proxies extraídos. Exec ID: {newId}");