using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HostingLib.Helpers
{
    public class RabbitMQConnectionHelper
    {
        public static readonly IModel channel;
        static RabbitMQConnectionHelper()
        {
            ConnectionFactory factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            channel = connection.CreateModel();
        }

    }
}
