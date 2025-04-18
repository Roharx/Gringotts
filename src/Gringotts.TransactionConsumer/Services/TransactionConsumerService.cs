using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Gringotts.LedgerService.Data;
using Gringotts.Shared.Models;

namespace Gringotts.TransactionConsumer.Services
{
    public class TransactionConsumerService : BackgroundService
    {
        private readonly ILogger<TransactionConsumerService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IModel _channel;
        private const string QueueName = "transactionQueue";

        // Track retry counts by Transaction Id.
        private static readonly ConcurrentDictionary<Guid, int> _retryCounts = new ConcurrentDictionary<Guid, int>();

        public TransactionConsumerService(ILogger<TransactionConsumerService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq",
                Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672")
            };

            const int maxRetries = 5;
            const int delayMilliseconds = 5000;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclare(
                        queue: QueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                    _logger.LogInformation("Successfully connected to RabbitMQ.");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Attempt {attempt} of {maxRetries} failed: {ex.Message}");
                    Thread.Sleep(delayMilliseconds);
                }
            }
            throw new Exception("Could not establish connection to RabbitMQ after multiple attempts.");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    return;
                }

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Received message: {message}");

                try
                {
                    var transaction = JsonSerializer.Deserialize<Transaction>(message);
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
                        dbContext.Transactions.Add(transaction);
                        dbContext.SaveChanges();
                    }
                    // If processing is successful, acknowledge and remove any retry record.
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    if (transaction != null)
                    {
                        _retryCounts.TryRemove(transaction.Id, out _);
                    }
                }
                catch (Exception ex)
                {
                    // Try to extract the transaction Id from the message.
                    Guid transactionId = Guid.Empty;
                    try
                    {
                        var temp = JsonSerializer.Deserialize<Transaction>(message);
                        if (temp != null)
                            transactionId = temp.Id;
                    }
                    catch { }

                    // Update the retry count if a valid Id is present.
                    int retryCount = 0;
                    if (transactionId != Guid.Empty)
                    {
                        retryCount = _retryCounts.AddOrUpdate(transactionId, 1, (id, current) => current + 1);
                    }

                    if (retryCount >= 5)
                    {
                        _logger.LogError(ex, $"Error processing message for transaction {transactionId}; reached max retry attempts. Dropping message.");
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        if (transactionId != Guid.Empty)
                        {
                            _retryCounts.TryRemove(transactionId, out _);
                        }
                    }
                    else
                    {
                        _logger.LogError(ex, $"Error processing message for transaction {transactionId}; retry attempt {retryCount}. Requeueing message.");
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
