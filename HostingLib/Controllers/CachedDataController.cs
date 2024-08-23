using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HostingLib.Classes;
using HostingLib.Helpers;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace HostingLib.Controllers
{
    public class CachedDataController
    {
        private static readonly IDatabase redis = RedisConnectionHelper.Connection.GetDatabase();

        public static async Task<FileDetails> GetFileDetailsAsync(int file_id)
        {
            string cache_key = $"file:details:{file_id}";

            RedisValue cached_details = await redis.StringGetAsync(cache_key);

            return !cached_details.IsNullOrEmpty ? JsonConvert.DeserializeObject<FileDetails>(cached_details) : null;

        }

        public static async Task SetFileDetailsAsync(int file_id, FileDetails details)
        {
            string cache_key = $"file:details:{file_id}";
            await redis.StringSetAsync(cache_key, JsonConvert.SerializeObject(details), TimeSpan.FromHours(1));
        }
    }
}
