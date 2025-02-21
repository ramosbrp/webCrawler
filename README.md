# WebCrawler .NET

Este projeto **WebCrawler** foi desenvolvido em **.NET 9** como parte de uma prova / demonstração de conhecimento. O crawler acessa o site [https://proxyservers.pro/proxy/list/order/updated/order_dir/desc](https://proxyservers.pro/proxy/list/order/updated/order_dir/desc) para extrair proxies (campos como IP Address, Port, Country, Protocol) e salvar as informações em arquivos e banco de dados.

---

## Índice

1. Principais Funcionalidades  
2. Arquitetura  
3. Tecnologias Utilizadas  
4. Estrutura de Pastas  
5. Como Executar  
6. Como Rodar Testes  
7. Detalhes de Configuração (appsettings.json)  
8. Demonstração de Uso  
9. Futuros Melhoramentos


## Principais Funcionalidades

1. **WebCrawler** para o site [proxyservers.pro](https://proxyservers.pro/proxy/list/order/updated/order_dir/desc).  
2. **Extração dos campos**:
   - IP Address  
   - Port  
   - Country  
   - Protocol  
3. **Multithread**:
   - Possui até 3 execuções simultâneas na abordagem de fila (ou orquestrador).
   - Ou internamente processando páginas com paralelismo de até 3 threads.
4. **Print das páginas** (HTML) em disco, salvando o conteúdo de cada página em arquivo `.html`.
5. **Salvamento** em arquivo JSON dos proxies coletados.
6. **Registro em Banco de Dados** (SQL Server) com EF Core, contendo:
   - Data/hora de início  
   - Data/hora de término  
   - Quantidade de páginas processadas  
   - Quantidade total de linhas extraídas  
   - Caminho do arquivo JSON gerado
7. **Testes de Unidade** e alguns **Testes de Integração** usando xUnit e Moq.


## Arquitetura

Este projeto segue uma **Arquitetura Hexagonal** (ou “Ports and Adapters”) com divisão em camadas/projetos:

- **WebCrawler.Domain**  
  - Modelos (entidades) principais: `ProxyInfo`, `CrawlerExecution`.  
  - Definições de Portas (Interfaces): `IHtmlDownloader`, `IProxyParser`, `IProxyRepository`, `IPagePrinter`, etc.

- **WebCrawler.Application**  
  - Serviços de aplicação, casos de uso do sistema.  
  - `CrawlerService` (onde a lógica principal de extração/paginação/multithread fica).  
  - Interfaces como `ICrawlerService`.  
  - Ponto ideal para colocar **DTOs** (ex.: `CrawlerRunResult`).

- **WebCrawler.Infrastructure**  
  - Implementações concretas (adapters) das portas do domínio/aplicação:
    - `HtmlAgilityDownloader` (implementa `IHtmlDownloader`, usando Html Agility Pack ou HttpClient).  
    - `HtmlAgilityProxyParser` (implementa `IProxyParser`).  
    - `FilePagePrinter` (implementa `IPagePrinter` para salvar `.html`).  
    - `JsonProxyFileExporter` (implementa `IProxyFileExporter`).  
    - `SqlProxyRepository` / `EfProxyRepository` (implementa `IProxyRepository`, usando EF Core / Dapper).  
  - `CrawlerDbContext` (DbContext do EF Core, mapeando as tabelas `CrawlerExecutions` e `Proxies`).

- **WebCrawler.UI**  
  - Projeto **Console** (ou Worker Service) que contém o `Program.cs`, injeta as dependências e orquestra a execução do crawler.  
  - Lê `appsettings.json` (connection string, etc.) e cria o `HostBuilder`.  
  - Chama `ICrawlerService.RunCrawlerAsync()` e salva logs em banco, JSON, etc.

### Diagrama de Dependências
```
Domain <---- Application <---- UI
   ^            ^
   |            |
   +---- Infrastructure

```


## Tecnologias Utilizadas

1. **.NET 9** (C# 11)  
2. **Entity Framework Core** (EF Core) para persistência em SQL Server  
3. **Html Agility Pack** (ou HttpClient) para download e parse de HTML  
4. **xUnit** + **Moq** para testes  
5. **Microsoft.Extensions.Hosting** e **Microsoft.Extensions.DependencyInjection** para DI e orquestração  
6. **Microsoft.Extensions.Logging** para logs em console


## Estrutura de Pastas
```
|- WebCrawler.Application
	|-CrawlerService
	|-ICrawlerService
|- WebCrawler.Domain
	|- Models
		|- CrawlerExecution
		|- ProxyInfo
	|- Ports
		|- IHtmlDownloader
		|- IProxyFileExporter
		|- IProxyParser
		|- IProxyRepository
|- WebCrawler.Infrastructure
	|- Migrations
	|- Persistence
		|- CrawlerDbContext
		|- SqlProxyRepository
	|- HtmlAgilityDownloader
	|- HtmlAgilityProxyParser 
	|- JsonProxyFileExporter
	|- SqlProxyRepository 
|- WebCrawler.UI
	|-Program.cs



```


*Observação: a organização pode variar, mas essa é uma referência comum.*


## Como Executar

1. **Clonar o Repositório**

 ```bash
 git clone https://github.com/seu-usuario/WebCrawler.git
 cd WebCrawler
 ```
   
2. **Restaurar Dependências**

```
dotnet restore
```

3. **Configurar a String de Conexão**
Abra appsettings.json no projeto WebCrawler.UI e edite a DefaultConnection para apontar para seu SQL Server ou LocalDB. Exemplo:

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=WebCrawlerDb;Trusted_Connection=True;"
  }
}
```
4. **Criar e Atualizar o Banco**
Se estiver usando EF Core, execute as migrations:

```
dotnet ef migrations add InitialCreate --project WebCrawler.Infrastructure --startup-project WebCrawler.UI
dotnet ef database update --project WebCrawler.Infrastructure --startup-project WebCrawler.UI

```

5. **Executar o Projeto (Console)**
```
cd WebCrawler.UI
dotnet run

```

Isso iniciará o crawler, que:

 - Baixará as páginas.
 - Salvará .html (print) em uma pasta, ex. PagesHtml.
 - Parseará e salvará proxies em um arquivo JSON.
 - Inserirá um registro em CrawlerExecutions no banco de dados.


## Demonstração de Uso
1. **Execução Única**
   - Ao rodar dotnet run (no projeto WebCrawler.UI), ele faz um loop, extrai proxies até encontrar uma página vazia (ou processar todas).
   - Salva .html de cada página na pasta (ex. PagesHtml).
   - Gera proxies.json e registra a execução no banco, preenchendo CrawlerExecutions.

2. **Execuções Simultâneas**
   - É possível disparar múltiplas execuções do crawler se desejar, mas por padrão o código limita a 3 em paralelo (se configurado com SemaphoreSlim ou “fila” com 3 workers internos).

3. **Paginação Paralela**
   - Dependendo da configuração, o crawler pode rodar com 3 threads processando as páginas, enfileirando “page+1” conforme encontra proxies.







