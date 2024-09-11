using HostingLib.Helpers;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Collections.Generic;

namespace HostingLib.Controllers
{
    public class QueueController 
    {
        private static readonly Dictionary<string, Action<string>> queue_handlers;

        // static QueueController()
        // {
        //     queue_handlers = new Dictionary<string, Action<string>>
        //     {
        //         { "EraseFolderQueue", EraseFolder},
        //     };

        //     foreach (var queue_name in queue_handlers.Keys)
        //     {
        //         RabbitMQConnectionHelper.channel.QueueDeclare(
        //             queue: queue_name, 
        //             durable: false, 
        //             exclusive: false, 
        //             autoDelete: false, 
        //             arguments: null);
                    
        //         EventingBasicConsumer consumer = new(RabbitMQConnectionHelper.channel);
        //         consumer.Received += (model, ea) =>
        //         {
        //             byte[] body = ea.Body.ToArray();
        //             string message = Encoding.UTF8.GetString(body);
        //             queue_handlers[queue_name]?.Invoke(message);
        //         };

        //         RabbitMQConnectionHelper.channel.BasicConsume(queue: queue_name, autoAck: true, consumer: consumer);
        //     }

        // }

    //     public static void Produce(string queue, string message)
    //     {
    //         RabbitMQConnectionHelper.channel.QueueDeclare(queue: queue,
    //                  durable: false,
    //                  exclusive: false,
    //                  autoDelete: false,
    //                  arguments: null);

    //         byte[] body = Encoding.UTF8.GetBytes(message);
    //         RabbitMQConnectionHelper.channel.BasicPublish(exchange: "",
    //                        routingKey: queue,
    //                         mandatory: true,
    //                        basicProperties: null,
    //                        body: body);
    //     }

    //     public static void Consume(string queue)
    //     {
    //         RabbitMQConnectionHelper.channel.QueueDeclare(queue: queue,
	// 						 durable: false,
	// 						 exclusive: false,
	// 						 autoDelete: false,
	// 						 arguments: null);
            
    //         EventingBasicConsumer consumer = new(RabbitMQConnectionHelper.channel);
    //         consumer.Received += (model, ea) => 
    //         {
    //             byte[] body = ea.Body.ToArray();
    //             string message = Encoding.UTF8.GetString(body);
    //             Console.WriteLine($" [x] Received {message}");
    //         };

    //         string consumer_tag = RabbitMQConnectionHelper.channel.BasicConsume(queue: queue,
    //                  autoAck: true,
    //                  consumerTag: consumer.Tag,
    //                  consumer: consumer);
    //     }
    }
}