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
        private static readonly Lazy<ConnectionMultiplexer> lazy_connection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect("localhost");
        });

        private static ConnectionMultiplexer Connection => lazy_connection.Value;
        public static readonly IDatabase db = Connection.GetDatabase();
    }
}
