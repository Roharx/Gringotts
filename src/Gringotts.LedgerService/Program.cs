using Microsoft.EntityFrameworkCore;
using Gringotts.LedgerService.Data;
using Gringotts.LedgerService.Services;
using Gringotts.LedgerService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Configure database connection.
var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "db";
var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "gringottsdb";
var user = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "gringotts";
var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "gringotts";
var connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}";

builder.Services.AddDbContext<LedgerDbContext>(options =>
    options.UseNpgsql(connectionString));

// W3C Context Propagation
Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(new TextMapPropagator[]
{
    new TraceContextPropagator(),
    new BaggagePropagator()
}));

// Configure OpenTelemetry Tracing for LedgerService using the new API.
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("LedgerService"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddJaegerExporter(o =>
            {
                o.AgentHost = Environment.GetEnvironmentVariable("JAEGER_AGENT_HOST") ?? "jaeger";
                o.AgentPort = int.Parse(Environment.GetEnvironmentVariable("JAEGER_AGENT_PORT") ?? "6831");
            });
    });

// Add controllers and Swagger services.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
// Program.cs (before builder.Build())
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();


var app = builder.Build();

// Enable Prometheus metrics.
app.UseMetricServer();  // Exposes /metrics
app.UseHttpMetrics();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();