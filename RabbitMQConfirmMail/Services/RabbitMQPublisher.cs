using RabbitMQConfirmMail.Models;
using System.Text;
using System.Text.Json;

namespace RabbitMQConfirmMail.Services
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService _rabbitmqClientService;
        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            _rabbitmqClientService = rabbitMQClientService;
        }

        public void Publish(CreateConfirmationCode createConfirmationCode)
        {
            var channel = _rabbitmqClientService.Connect();
            var bodyString = JsonSerializer.Serialize(createConfirmationCode);
            var bodyByte = Encoding.UTF8.GetBytes(bodyString);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            channel.BasicPublish(RabbitMQClientService.exchangeName,RabbitMQClientService.mailRouteKey,false,properties,bodyByte);

        }
    }
}
