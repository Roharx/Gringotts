using System.Net.Http.Json;
using Gringotts.Shared.Models;
using Gringotts.Shared.Models.CurrencyService;
using Gringotts.Shared.Models.LedgerService.TransactionService;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5002/") }; // LedgerService
var baseUrl = "http://localhost:5002";
Console.WriteLine("Sending test traffic to LedgerService...");

for (int i = 0; i < 10; i++)
{
    var transaction = new Transaction
    {
        Type = TransactionType.Credit,
        DkkAmount = 100 + i,
        Amount = new Money { Galleons = i, Sickles = i * 2, Knuts = i * 3 },
        Date = DateTime.UtcNow
    };

    var response = await client.PostAsJsonAsync($"{baseUrl}/api/transactions", transaction);

    Console.WriteLine($"[#{i}] Status: {response.StatusCode}");
    await Task.Delay(200); // Simulate traffic over time
}

Console.WriteLine("Done generating traffic.");