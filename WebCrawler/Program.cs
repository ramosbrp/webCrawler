using System;
using WebCrawler.Services;

// 1. Criar HttpClient
var httpClient = new HttpClient();

// 2. Instanciar Service
var crawlerService = new CrawlerService(httpClient);

string url = $"https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/";

var proxies = await crawlerService.ExtractProxiesFromPageAsync(url);