﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebCrawler.Application;
using WebCrawler.Domain.Ports;
using WebCrawler.Infrastructure;
using WebCrawler.Infrastructure.Persistence;




// Cria um Host que gerencia DI
var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        // Se quiser logar em console ou em outros providers
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureServices((context, services) =>
    {
        // Lê a connection string do appsettings.json
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("A string de conexão não foi carregada. Verifique o appsettings.json.");

        // Adiciona DbContext
        services.AddDbContext<CrawlerDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Adicionar implementações concretas para interfaces
        services.AddTransient<IHtmlDownloader, HtmlAgilityDownloader>();
        services.AddTransient<ICrawlerService, CrawlerService>();
        services.AddTransient<IProxyFileExporter, JsonProxyFileExporter>();
        services.AddTransient<IProxyParser, HtmlAgilityProxyParser>();
        services.AddTransient<IPagePrinter, FilePagePrinter>();
        services.AddTransient<IProxyRepository, SqlProxyRepository>();

    })
    .Build();
try
{
    using var scope = host.Services.CreateScope();
    var crawlerService = host.Services.GetRequiredService<ICrawlerService>();
    var exporter = host.Services.GetRequiredService<IProxyFileExporter>();
    var repo = host.Services.GetRequiredService<IProxyRepository>();


    // Início da execução do Crawler
    var startTime = DateTime.Now;
    Console.WriteLine($"Iniciando crawler em {startTime}");
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
        PagesCount = pagesCount,
        TotalRecords = proxies.Count,
        JsonFilePath = filePath
    };

    Console.WriteLine($"Salvando log da execução no banco de dados - {DateTime.Now}");
    var newId = await repo.SaveExecutionAsync(exec);

    Console.WriteLine($"Execução finalizada. {proxies.Count} proxies extraídos. Exec ID: {newId}");
}
catch (Exception ex)
{
    // Aqui capturamos qualquer exceção não tratada
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"ERRO FATAL: {ex.Message}");
    Console.ResetColor();

    // Encerrar a aplicação com código de erro:
    Environment.Exit(-1);
}
