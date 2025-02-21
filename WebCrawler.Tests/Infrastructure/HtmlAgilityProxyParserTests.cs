namespace WebCrawler.Tests.Infrastructure;

using WebCrawler.Infrastructure;
using Xunit;

public class HtmlAgilityProxyParserTests
{
    [Fact]
    public void ParseProxies_ShouldReturnListOfProxies_WhenHtmlIsValid()
    {
        // Arrange
        var parser = new HtmlAgilityProxyParser(); // Supondo que essa classe exista
        string sampleHtml = @"
                <tbody>
                    <tr>
                        <td class='text-nowrap'>21 min</td>
                        <td> <a href='/proxy/1.2.3.4'>1.2.3.4</a> </td>
                        <td> <span class='port' data-port='0B07'></span> </td>
                        <td class='text-nowrap'>Brasil</td>
                        <td></td>
                        <td></td>
                        <td>HTTPS</td>
                        <td>Transparent</td>
                    </tr>
                </tbody>";

        // Act
        var result = parser.ParseProxies(sampleHtml);

        // Assert
        Assert.Single(result); // Deve ter 1 registro
        var proxy = result[0];
        Assert.Equal("1.2.3.4", proxy.IpAddress);
        Assert.Equal("0B07", proxy.Port);
        Assert.Equal("Brasil", proxy.Country);
        Assert.Equal("HTTPS", proxy.Protocol);
    }

    [Fact]
    public void ParseProxies_ShouldReturnEmpty_WhenHtmlHasNoRows()
    {
        // Arrange
        var parser = new HtmlAgilityProxyParser();
        string emptyHtml = "<html><body><tbody></tbody></body></html>";

        // Act
        var result = parser.ParseProxies(emptyHtml);

        // Assert
        Assert.Empty(result);
    }
}
