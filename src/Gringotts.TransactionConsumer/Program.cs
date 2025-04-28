using Microsoft.EntityFrameworkCore;
using Gringotts.LedgerService.Data;
using Gringotts.TransactionConsumer.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Prometheus;

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

        // Register Prometheus metrics for injection
        var messageCounter = Metrics.CreateCounter("transaction_consumer_messages_total", "Total messages consumed");
        var processingDuration = Metrics.CreateHistogram("transaction_consumer_processing_duration_seconds", "Duration of processing messages");

        services.AddSingleton(messageCounter);
        services.AddSingleton(processingDuration);

        // Register the transaction consumer service
        services.AddHostedService<TransactionConsumerService>();

        // Register Prometheus metrics server on port 8080
        new KestrelMetricServer(port: 8080).Start();

        // Propagation context for OpenTelemetry
        Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(new TextMapPropagator[]
        {
            new TraceContextPropagator(),
            new BaggagePropagator()
        }));

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
