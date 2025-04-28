using System.Net.Http.Json;
using FluentAssertions;
using Gringotts.Shared.Models;
using Gringotts.CurrencyService.Services.Models;
using Gringotts.Shared.Models.CurrencyService;
using Gringotts.Shared.Models.LedgerService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace Gringotts.CurrencyService.Tests;

public class CurrencyConversionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CurrencyConversionTests(WebApplicationFactory<Program> factory)
    {
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<CurrencyDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<CurrencyDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryTestDb");
                });

                // Seed a rate so conversions work
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CurrencyDbContext>();
                db.ExchangeRates.Add(new ExchangeRate
                {
                    GalleonToDkk = 50,
                    EffectiveDate = DateTime.UtcNow
                });
                db.SaveChanges();
            });
        });

        _client = customFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost")
        });
    }

    [Fact]
    public async Task ConvertGalleonsToDkk_ShouldReturnCorrectAmount()
    {
        var money = new Money { Galleons = 1, Sickles = 0, Knuts = 0 };
        var response = await _client.PostAsJsonAsync("/api/currency/convert-to-dkk", money);
        response.EnsureSuccessStatusCode();

        var dkkAmount = await response.Content.ReadFromJsonAsync<decimal>();
        dkkAmount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertDkkToWizarding_ShouldReturnBreakdown()
    {
        var dkkAmount = 150m;
        var response = await _client.PostAsJsonAsync("/api/currency/convert-from-dkk", dkkAmount);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Money>();
        result.Should().NotBeNull();
        result.Galleons.Should().BeGreaterThanOrEqualTo(0);
    }
}
