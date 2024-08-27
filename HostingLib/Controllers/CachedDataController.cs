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
        #region User

        #endregion

        #region File

        public static async Task<FileDetails> GetFileDetailsAsync(int file_id)
        {
            string cache_key = $"file:details:{file_id}";

            RedisValue cached_details = await RedisConnectionHelper.db.StringGetAsync(cache_key);

            return !cached_details.IsNullOrEmpty ? JsonConvert.DeserializeObject<FileDetails>(cached_details) : null;

        }

        public static async Task SetFileDetailsAsync(int file_id, FileDetails details)
        {
            string cache_key = $"file:details:{file_id}";
            await RedisConnectionHelper.db.StringSetAsync(cache_key, JsonConvert.SerializeObject(details), TimeSpan.FromHours(1));
        }

        public static async Task ScheduleFileDeletionAsync(string file_id, TimeSpan delay)
        {
            string cache_key = $"deleted_files";

            long deletion_time = DateTimeOffset.UtcNow.Add(delay).ToUnixTimeSeconds();
            await RedisConnectionHelper.db.SortedSetAddAsync(cache_key, file_id, deletion_time);
            LoggingController.LogInfo($"Scheduled file {file_id} for deletion at {deletion_time}");
        }

        #endregion

    }
}
