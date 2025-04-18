using Gringotts.CurrencyService.Services;
using Gringotts.CurrencyService.Services.Interfaces;
using Gringotts.CurrencyService.Services.Models;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using System.Runtime.CompilerServices;

// Make it visible to tests
[assembly: InternalsVisibleTo("Gringotts.CurrencyService.Tests")]

var builder = WebApplication.CreateBuilder(args);
var isTesting = builder.Environment.IsEnvironment("Testing");
if (!isTesting)
{
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "db";
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
    var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "gringotts";
    var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "gringotts";
    var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "gringotts";

    var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

    builder.Services.AddDbContext<CurrencyDbContext>(opt =>
        opt.UseNpgsql(connectionString));
}

builder.Services.AddScoped<ICurrencyConverter, CurrencyConverter>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Monitoring: Prometheus & Jaeger
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("CurrencyService"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        if (!builder.Environment.IsEnvironment("Testing")) // only add Jaeger if not in Testing env
        {
            tracerProviderBuilder.AddJaegerExporter(o =>
            {
                o.AgentHost = Environment.GetEnvironmentVariable("JAEGER_AGENT_HOST") ?? "jaeger";
                o.AgentPort = int.Parse(Environment.GetEnvironmentVariable("JAEGER_AGENT_PORT") ?? "6831");
            });
        }
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();

// Prometheus metrics
app.UseMetricServer(); // exposes /metrics
app.UseHttpMetrics();

app.MapControllers();
app.Run();

public partial class Program { }