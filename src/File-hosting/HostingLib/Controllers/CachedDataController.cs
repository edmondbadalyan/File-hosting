using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HostingLib.Classes;
using HostingLib.Helpers;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace HostingLib.Controllers
{
    public class CachedDataController
    {
        public static async Task SetValueAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            await RedisConnectionHelper.db.StringSetAsync(key, JsonConvert.SerializeObject(value), expiration);
            LoggingController.LogInfo($"CachedDataController.SetValueAsync - Set data at key {key} with expiration date {expiration}");
        }

        public static async Task<T> GetValueAsync<T>(string key)
        {
            string json = await RedisConnectionHelper.db.StringGetAsync(key);
            if (json.IsNullOrEmpty())
            {
                LoggingController.LogInfo($"CachedDataController.GetValueAsync - Getting data at key {key} - returned null");
                return default;
            }
            LoggingController.LogInfo($"CachedDataController.SetValueAsync - Getting data at key {key} - data found");
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task RemoveCacheAsync(string key)
        {
            await RedisConnectionHelper.db.KeyDeleteAsync(key);
            LoggingController.LogInfo($"CachedDataController.RemoveCacheAsync - Removed key {key}");
        }

        public static async Task ScheduleFileDeletionAsync(string file_id, TimeSpan delay)
        {
            string cache_key = $"deleted_files";

            long deletion_time = DateTimeOffset.UtcNow.Add(delay).ToUnixTimeSeconds();
            await RedisConnectionHelper.db.SortedSetAddAsync(cache_key, file_id, deletion_time);
            LoggingController.LogInfo($"Scheduled file {file_id} for deletion at {deletion_time}");
        }

    }
}
