using System.Text;
using AspNetFileUpload.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace AspNetFileUpload.Rabbit
{
    public class RabbitMqAccessLayer: IMessageQueueAccessLayer
    {
        private readonly IModel _channel;
        private readonly IBasicProperties _basicProperties;

        public RabbitMqAccessLayer(IConfiguration config)
        {
            var factory = new ConnectionFactory
            {
                HostName = config["RabbitMq:HostName"].Default("localhost"),
                Port = config["RabbitMq:Port"].ToInt(5672),
                UserName = config["RabbitMq:UserName"].Default("guest"),
                Password = config["RabbitMq:Password"].Default("guest")
            };

            var connection = factory.CreateConnection();

            _channel = connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "moorea", type: "direct");

            _basicProperties = _channel.CreateBasicProperties();
            _basicProperties.Persistent = false;
        }
        
        public bool SendAction(IMessageQueueBaseAction action)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(action));
            _channel.BasicPublish("moorea", "moorea-actions", _basicProperties, body);

            return true;
        }
    }
}