using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Gringotts.TransactionPublisher.Services
{
    public class TransactionMessageProducer
    {
        private readonly ConnectionFactory _factory;
        private const string QueueName = "transactionQueue";

        public TransactionMessageProducer(string host, int port)
        {
            _factory = new ConnectionFactory() { HostName = host, Port = port };
        }

        public void PublishTransaction<T>(T transactionMessage)
        {
            var messageBody = JsonSerializer.Serialize(transactionMessage);
            var body = Encoding.UTF8.GetBytes(messageBody);

            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declare a durable queue
            channel.QueueDeclare(queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true; // persist messages to disk

            channel.BasicPublish(exchange: "",
                routingKey: QueueName,
                basicProperties: properties,
                body: body);
        }
    }
}