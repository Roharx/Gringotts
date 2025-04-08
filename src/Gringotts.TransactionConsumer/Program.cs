using Microsoft.EntityFrameworkCore;
using Gringotts.LedgerService.Data;
using Gringotts.TransactionConsumer.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Build the connection string from environment variables.
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "db";
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "gringottsdb";
        var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "gringotts";
        var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "gringotts";
        var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

        // Register LedgerDbContext.
        services.AddDbContext<LedgerDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register the transaction consumer service.
        services.AddHostedService<TransactionConsumerService>();

        // Configure OpenTelemetry Tracing for the TransactionConsumer.
        services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("TransactionConsumer"))
                    .AddHttpClientInstrumentation()
                    .AddJaegerExporter(o =>
                    {
                        o.AgentHost = Environment.GetEnvironmentVariable("JAEGER_AGENT_HOST") ?? "jaeger";
                        o.AgentPort = int.Parse(Environment.GetEnvironmentVariable("JAEGER_AGENT_PORT") ?? "6831");
                    });
            });
    })
    .Build();

await host.RunAsync();
