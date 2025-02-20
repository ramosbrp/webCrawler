using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebCrawler.Application;
using WebCrawler.Domain.Ports;
using WebCrawler.Infrastructure;
using WebCrawler.Infrastructure.Persistence;




// Cria um Host que gerencia DI
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Lê a connection string do appsettings.json
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("A string de conexão não foi carregada. Verifique o appsettings.json.");
        }

        // Adiciona DbContext
        services.AddDbContext<CrawlerDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Adicionar implementações concretas para interfaces
        services.AddTransient<IHtmlDownloader, HtmlAgilityDownloader>();
        services.AddTransient<ICrawlerService, CrawlerService>();
        services.AddTransient<IProxyFileExporter, JsonProxyFileExporter>();
        services.AddTransient<IProxyParser, HtmlAgilityProxyParser>();
        services.AddTransient<IProxyRepository, SqlProxyRepository>();

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
var crawlerResult = await crawlerService.RunCrawlerAsync();
var proxies = crawlerResult.Proxies;
var pagesCount = crawlerResult.PagesCount;

// Salva em JSON
Console.WriteLine($"Salvando proxies obtidos em Json - {DateTime.Now}");
var filePath = await exporter.SaveProxiesToJsonAsync(proxies);

var endTime = DateTime.Now;

// Salva log de execução no banco
var exec = new WebCrawler.Domain.Models.CrawlerExecution
{
    StartTime = startTime,
    EndTime = endTime,
    PagesCount = pagesCount, // Ajustar para a contagem real 
    TotalRecords = proxies.Count,
    JsonFilePath = filePath
};

Console.WriteLine($"Salvando log da execução no banco de dados - {DateTime.Now}");
var newId = await repo.SaveExecutionAsync(exec);

Console.WriteLine($"Execução finalizada. {proxies.Count} proxies extraídos. Exec ID: {newId}");