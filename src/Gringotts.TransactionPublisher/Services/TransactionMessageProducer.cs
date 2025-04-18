using System.Diagnostics;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

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

            var activity = new Activity("PublishTransaction");
            activity.Start();

            var propagator = Propagators.DefaultTextMapPropagator;
            var props = new Dictionary<string, object>();

            propagator.Inject(new PropagationContext(activity.Context, Baggage.Current), props, InjectTraceContextToDictionary);

            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Headers = props.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);

            channel.BasicPublish(exchange: "",
                routingKey: QueueName,
                basicProperties: properties,
                body: body);

            activity.Stop();
        }
        
        private static void InjectTraceContextToDictionary(IDictionary<string, object> props, string key, string value)
        {
            props[key] = value;
        }


    }
}