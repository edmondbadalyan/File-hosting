using HostingLib.Helpers;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace HostingLib.Controllers
{
    public class QueueController 
    {
        public static void Produce(string queue, string message)
        {
            RabbitMQConnectionHelper.channel.QueueDeclare(queue: queue,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

            byte[] body = Encoding.UTF8.GetBytes(message);
            RabbitMQConnectionHelper.channel.BasicPublish(exchange: "",
                           routingKey: queue,
                            mandatory: true,
                           basicProperties: null,
                           body: body);
        }

        public static void Consume(string queue)
        {
            RabbitMQConnectionHelper.channel.QueueDeclare(queue: queue,
							 durable: false,
							 exclusive: false,
							 autoDelete: false,
							 arguments: null);
            
            EventingBasicConsumer consumer = new(RabbitMQConnectionHelper.channel);
            consumer.Received += (model, ea) => 
            {
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Received {message}");
            };

            string consumer_tag = RabbitMQConnectionHelper.channel.BasicConsume(queue: queue,
                     autoAck: true,
                     consumer: consumer);
        }
    }
}