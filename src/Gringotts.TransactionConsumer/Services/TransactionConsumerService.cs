using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Gringotts.LedgerService.Data;
using Gringotts.Shared.Models;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using Gringotts.Shared.Models.LedgerService.TransactionService;
using OpenTelemetry;
using Prometheus;

namespace Gringotts.TransactionConsumer.Services
{
    public class TransactionConsumerService : BackgroundService
    {
        private static readonly ActivitySource ActivitySource = new("TransactionConsumer");
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        private readonly ILogger<TransactionConsumerService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Counter _messageCounter;
        private readonly Histogram _processingDuration;
        private IConnection _connection;
        private IModel _channel;
        private const string QueueName = "transactionQueue";
        private static readonly ConcurrentDictionary<Guid, int> _retryCounts = new();

        public TransactionConsumerService(
            ILogger<TransactionConsumerService> logger,
            IServiceScopeFactory scopeFactory,
            Counter messageCounter,
            Histogram processingDuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _messageCounter = messageCounter;
            _processingDuration = processingDuration;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq",
                Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var port) ? port : 5672
            };

            for (int attempt = 1; attempt <= 5; attempt++)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
                    _logger.LogInformation("Connected to RabbitMQ.");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Attempt {attempt} failed: {ex.Message}");
                    Thread.Sleep(5000);
                }
            }

            throw new Exception("Could not connect to RabbitMQ after multiple attempts.");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var props = ea.BasicProperties;
                var parentContext = Propagator.Extract(default, props, (p, k) =>
                {
                    if (p.Headers != null && p.Headers.TryGetValue(k, out var value))
                    {
                        var bytes = value as byte[];
                        return bytes != null ? new[] { Encoding.UTF8.GetString(bytes) } : Array.Empty<string>();
                    }
                    return Array.Empty<string>();
                });

                Baggage.Current = parentContext.Baggage;

                using var activity = ActivitySource.StartActivity("Consume Transaction", ActivityKind.Consumer, parentContext.ActivityContext);
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received: {Message}", message);

                // Record the metrics
                _messageCounter.Inc();
                using var timer = _processingDuration.NewTimer();

                try
                {
                    var transaction = JsonSerializer.Deserialize<Transaction>(message);

                    if (transaction != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
                        dbContext.Transactions.Add(transaction);
                        dbContext.SaveChanges();

                        _retryCounts.TryRemove(transaction.Id, out _);
                        _channel.BasicAck(ea.DeliveryTag, false);

                        activity?.SetTag("transaction.id", transaction.Id);
                        activity?.SetStatus(ActivityStatusCode.Ok);
                    }
                }
                catch (Exception ex)
                {
                    HandleError(ea, message, ex);
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        private void HandleError(BasicDeliverEventArgs ea, string message, Exception ex)
        {
            Guid transactionId = Guid.Empty;
            try
            {
                transactionId = JsonSerializer.Deserialize<Transaction>(message)?.Id ?? Guid.Empty;
            }
            catch { }

            int retryCount = 0;
            if (transactionId != Guid.Empty)
            {
                retryCount = _retryCounts.AddOrUpdate(transactionId, 1, (_, current) => current + 1);
            }

            if (retryCount >= 5)
            {
                _logger.LogError(ex, "Max retries for transaction {TransactionId}. Dropping.", transactionId);
                _channel.BasicAck(ea.DeliveryTag, false);
                _retryCounts.TryRemove(transactionId, out _);
            }
            else
            {
                _logger.LogError(ex, "Error on transaction {TransactionId}, retry {RetryCount}.", transactionId, retryCount);
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
