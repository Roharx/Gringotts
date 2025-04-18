using Gringotts.TransactionPublisher.Services;
using OpenTelemetry.Resources;
using Prometheus;
using OpenTelemetry.Trace;


var builder = WebApplication.CreateBuilder(args);

// Read RabbitMQ settings from configuration/environment variables.
var rabbitHost = builder.Configuration["RABBITMQ_HOST"] ?? "rabbitmq";
var rabbitPort = int.TryParse(builder.Configuration["RABBITMQ_PORT"], out var parsedPort) ? parsedPort : 5672;


// Register the TransactionMessageProducer service.
builder.Services.AddSingleton(new TransactionMessageProducer(rabbitHost, rabbitPort));

// Configure OpenTelemetry Tracing for TransactionPublisher.
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("TransactionPublisher"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddJaegerExporter(o =>
            {
                o.AgentHost = builder.Configuration["JAEGER_AGENT_HOST"] ?? "jaeger";
                o.AgentPort = int.Parse(builder.Configuration["JAEGER_AGENT_PORT"] ?? "6831");
            });
    });

// Register controllers and Swagger.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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