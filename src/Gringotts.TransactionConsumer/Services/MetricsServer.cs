namespace Gringotts.TransactionConsumer.Services;

public class MetricsServer : BackgroundService
{
    private readonly ILogger<MetricsServer> _logger;

    public MetricsServer(ILogger<MetricsServer> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var metricServer = new Prometheus.MetricServer(hostname: "*", port: 8080); // Port must match Docker!
        _logger.LogInformation("Starting Prometheus metrics server on port 8080");
        metricServer.Start();
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}