using RabbitMQ.Client;

namespace RabbitMQConfirmMail.Services
{

    public class RabbitMQClientService : IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        public static string exchangeName = "MailDirectExchange";
        public static string mailRouteKey = "confirm-mail-route";
        public static string queueName = "confirm-mail-queue";
        private readonly ILogger<RabbitMQClientService> _logger;
        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;

        }

        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();

            if (_channel != null && _channel.IsOpen)
                return _channel;

            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchangeName, type: ExchangeType.Direct, true, false);
            _channel.QueueDeclare(queueName, true, false, false, null);
            _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: mailRouteKey);
            _logger.LogInformation("RabbitMQ ile bağlantı kuruldu...");

            return _channel;

        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();


            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ ile bağlantı koptu...");
        }
    }
}

