using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Helpers
{
    public class RedisConnectionHelper
    {
        private static Lazy<ConnectionMultiplexer> lazy_connection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect("localhost");
        });

        public static ConnectionMultiplexer Connection => lazy_connection.Value;
    }
}
