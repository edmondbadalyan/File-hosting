using HostingLib.Controllers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostingLib.Helpers
{
    public static class FileDeletionHelper
    {
        public static async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                long current_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // Handle file deletions
                RedisValue[] files_to_delete = await RedisConnectionHelper.db.SortedSetRangeByScoreAsync("deleted_files", 0, current_time);
                foreach (RedisValue file_id in files_to_delete)
                {
                    await FileController.EraseFile((int)file_id, token);
                    await RedisConnectionHelper.db.SortedSetRemoveAsync("deleted_files", file_id);
                }

                // Handle folder deletions
                RedisValue[] folders_to_delete = await RedisConnectionHelper.db.SortedSetRangeByScoreAsync("deleted_folders", 0, current_time);
                foreach (RedisValue folder_id in folders_to_delete)
                {
                    await FileController.EraseFolder((int) folder_id, token);
                    await RedisConnectionHelper.db.SortedSetRemoveAsync("deleted_folders", folder_id);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), token);
            }
        }
    }
}
