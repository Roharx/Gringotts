using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Gringotts.Shared.Models;

namespace Gringotts.System.Tests;

public class TransactionFlowTests
{
    private readonly HttpClient _client = new() { BaseAddress = new Uri("http://localhost:5003") };

    [Fact]
    public async Task Full_Transaction_Flow_Should_Accept_Transaction()
    {
        var transaction = new Transaction
        {
            Type = TransactionType.Credit,
            DkkAmount = 150,
            Amount = new Money { Galleons = 1, Sickles = 2, Knuts = 3 },
            Date = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/transactions", transaction);
        response.EnsureSuccessStatusCode();

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }
    
    [Fact]
    public async Task Full_Transaction_Flow_Should_Store_Transaction_In_DB()
    {
        var transaction = new Transaction
        {
            Type = TransactionType.Credit,
            DkkAmount = 150,
            Amount = new Money { Galleons = 1, Sickles = 2, Knuts = 3 },
            Date = DateTime.UtcNow
        };

        // Post to TransactionPublisher
        var publisherClient = new HttpClient { BaseAddress = new Uri("http://localhost:5003") };
        var postResponse = await publisherClient.PostAsJsonAsync("/api/transactions", transaction);
        postResponse.EnsureSuccessStatusCode();
        postResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        // Wait a bit for consumer to process
        await Task.Delay(5000);

        // Get from LedgerService
        var ledgerClient = new HttpClient { BaseAddress = new Uri("http://localhost:5002") };
        var transactions = await ledgerClient.GetFromJsonAsync<List<Transaction>>("/api/transactions");

        transactions.Should().NotBeNull();
        transactions.Should().Contain(t =>
            t.Type == transaction.Type &&
            t.DkkAmount == transaction.DkkAmount &&
            t.Amount.Galleons == transaction.Amount.Galleons &&
            t.Amount.Sickles == transaction.Amount.Sickles &&
            t.Amount.Knuts == transaction.Amount.Knuts
        );
    }

}