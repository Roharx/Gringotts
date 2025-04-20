using System;
using System.Net.Sockets;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Gringotts.LedgerService.Data;
using Npgsql;

Console.WriteLine("Starting migrations...");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Build the connection string from environment variables
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "db";
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "gringottsdb";
        var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "gringotts";
        var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "gringotts";
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";


        // Register the LedgerDbContext with the Npgsql provider
        services.AddDbContext<LedgerDbContext>(options =>
            options.UseNpgsql(connectionString));
    })
    .Build();

// Retry logic for waiting until the database is ready
int attempts = 0;
const int maxAttempts = 10;
while (true)
{
    try
    {
        using (var scope = host.Services.CreateScope())
        {
            Thread.Sleep(5000);
            Console.WriteLine("Waiting 5 secounds for the database...");
            var dbContext = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
            Console.WriteLine("Applying Ledger migrations...");
            dbContext.Database.Migrate();
        }

        Console.WriteLine("Migrations applied successfully.");
        break;
    }
    catch (PostgresException ex) when (ex.SqlState == "57P03") // DB starting up
    {
        attempts++;
        if (attempts >= maxAttempts)
        {
            Console.WriteLine("Max attempts reached. Exiting.");
            throw;
        }

        Console.WriteLine($"Attempt {attempts} failed (database is starting up). Waiting before retrying...");
        Thread.Sleep(10000);
    }
    catch (SocketException ex)
    {
        attempts++;
        if (attempts >= maxAttempts)
        {
            Console.WriteLine("Max attempts reached. Exiting.");
            throw;
        }

        Console.WriteLine($"Attempt {attempts} failed (socket error: {ex.Message}). Waiting before retrying...");
        Thread.Sleep(2000);
    }
}