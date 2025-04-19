using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Gringotts.Shared.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace Gringotts.ApiGateway.Tests;

public class ApiGatewayTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiGatewayTests(WebApplicationFactory<Program> factory)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        // Mock ConvertToDkk
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.AbsolutePath.Contains("convert-to-dkk")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(100m)
            });

        // Mock GetTransactions
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.AbsolutePath.Contains("transactions")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new List<Transaction>())
            });

        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.AddSingleton(new HttpClient(mockHandler.Object)
                {
                    BaseAddress = new Uri("http://mockedservice")
                });

                services.AddSingleton<IHttpClientFactory>(sp =>
                {
                    var client = sp.GetRequiredService<HttpClient>();
                    var mockFactory = new Mock<IHttpClientFactory>();
                    mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
                    return mockFactory.Object;
                });
            });
        });

        _client = customFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost")
        });
    }

    [Fact]
    public async Task ConvertToDkk_ShouldReturnValue()
    {
        var money = new Money { Galleons = 2 };
        var response = await _client.PostAsJsonAsync("/api/gateway/convert-to-dkk", money);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<decimal>();
        result.Should().Be(100);
    }

    [Fact]
    public async Task GetTransactions_ShouldReturnList()
    {
        var response = await _client.GetAsync("/api/gateway/transactions");
        response.EnsureSuccessStatusCode();

        var transactions = await response.Content.ReadFromJsonAsync<List<Transaction>>();
        transactions.Should().NotBeNull();
        transactions.Should().BeEmpty(); // Based on mock
    }
}
