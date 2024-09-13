using HostingLib.Data.Context;
using HostingLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostingLib.Controllers
{
    public class UserController
    {
        private const long UserQuota = 16_106_127_360; // 15 GB
        public const string CachePrefix = "User:";

        public static async Task<long> GetAvailableSpaceAsync(int user_id, CancellationToken token)
        {
            string cache_key = $"{CachePrefix}space:{user_id}";
            long? cached_space = await CachedDataController.GetValueAsync<long>(cache_key);
            
            if (cached_space.HasValue && cached_space.Value != default)
            {
                LoggingController.LogInfo($"UserController.GetAvailableSpaceAsync - Cache hit for user {user_id} with available space {cached_space.Value}");
                return cached_space.Value;
            }

            try
            {
                return await CalculateAvailableSpaceAsync(user_id, token);
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"UserController.GetAvailableSpaceAsync - Error calculating available space for user {user_id}: {ex.Message}");
                throw;
            }
        }

        private static async Task<long> CalculateAvailableSpaceAsync(int user_id, CancellationToken token)
        {
            using HostingDbContext context = new();
            token.ThrowIfCancellationRequested();

            long user_space = await context.Files
                .Where(f => f.UserId == user_id && f.ParentId == null)
                .SumAsync(f => f.Size, token);

            long available_space = UserQuota - user_space;
            string cacheKey = $"{CachePrefix}space:{user_id}";
            await CachedDataController.SetValueAsync(cacheKey, available_space);

            LoggingController.LogInfo($"UserController.CalculateAvailableSpaceAsync - Calculated available space for user {user_id}: {available_space}");
            return available_space;
        }

        public static async Task<TimeSpan> GetAutoDeletionTimeAsync(int user_id, CancellationToken token)
        {
            string cache_key = $"{CachePrefix}delete_time:{user_id}";

            TimeSpan? cached_space = await CachedDataController.GetValueAsync<TimeSpan>(cache_key);

            if (cached_space.HasValue && cached_space.Value != default)
            {
                LoggingController.LogInfo($"UserController.GetAvailableSpaceAsync - Cache hit for user {user_id} with available space {cached_space.Value}");
                return cached_space.Value;
            }

            try
            {
                HostingDbContext context = new();

                return await context.Users.Where(u => u.Id == user_id).Select(u => u.AutoFileDeletionTime).SingleAsync(token);
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"UserController.GetAvailableSpaceAsync - Error calculating available space for user {user_id}: {ex.Message}");
                throw;
            }
        }


        public static async Task<User> GetUserAsync(string email, CancellationToken token)
        {
            using HostingDbContext context = new();
            token.ThrowIfCancellationRequested();

            string cache_key = $"{CachePrefix}{email}";

            User cached_user = await CachedDataController.GetValueAsync<User>(cache_key);

            if (cached_user is not null)
            {
                LoggingController.LogInfo($"UserController.GetUserAsync - Cache hit for user {email}");
                return cached_user;
            }

            try
            {
                User user = await context.Users.SingleOrDefaultAsync(u => u.Email == email, token);
                LoggingController.LogInfo($"UserController.GetUserAsync - Request for email {email} returned user ID: {user?.Id}");
                await CachedDataController.SetValueAsync(cache_key, user);
                return user;
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"UserController.GetUserAsync - Error fetching user with email {email}: {ex.Message}");
                throw;
            }
        }

        public static async Task<User> GetUserByIdAsync(int id, CancellationToken token)
        {
            using HostingDbContext context = new();
            token.ThrowIfCancellationRequested();

            try
            {
                User user = await context.Users.SingleOrDefaultAsync(u => u.Id == id, token);
                LoggingController.LogInfo($"UserController.GetUserByIdAsync - Request for id {id} returned user");
                return user;
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"UserController.GetUserAsync - Error fetching user with id {id}: {ex.Message}");
                throw;
            }
        }

        public static async Task CreateUserAsync(string email, string password, bool isPublic, TimeSpan? deletion_time, CancellationToken token)
        {
            using var context = new HostingDbContext();

            try
            {
                token.ThrowIfCancellationRequested();
                User existing_user = await GetUserAsync(email, token);

                if (existing_user != null)
                {
                    string error_message = $"UserController.CreateUserAsync - Error: user with email {email} already exists";
                    LoggingController.LogError(error_message);
                    throw new InvalidOperationException(error_message);
                }

                User new_user = new(email, BCrypt.Net.BCrypt.HashPassword(password), true, isPublic, deletion_time is null ? TimeSpan.FromHours(23) : deletion_time);
                context.Users.Add(new_user);
                await context.SaveChangesAsync(token);

                CreateDirectoriesForUser(new_user.Id);

                LoggingController.LogInfo($"UserController.CreateUserAsync - Successfully created user with email: {email}");
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"UserController.CreateUserAsync - Error creating user with email {email}: {ex.Message}");
                throw;
            }
        }

        private static void CreateDirectoriesForUser(int user_id)
        {
            try
            {
                string user_directory = Path.Combine(FileController.StoragePath, user_id.ToString());
                Directory.CreateDirectory(user_directory);
                Directory.CreateDirectory(Path.Combine(user_directory, "Deleted"));
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"UserController.CreateDirectoriesForUser - Error creating directories for user ID {user_id}: {ex.Message}");
                throw;
            }
        }

        public static async Task UpdateUserAsync(User user, string new_password, CancellationToken token)
        {
            using HostingDbContext context = new();

            try
            {
                token.ThrowIfCancellationRequested();
                user.Password = BCrypt.Net.BCrypt.HashPassword(new_password);

                context.Users.Update(user);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"UserController.UpdateUserAsync - Updated password for user {user.Email}");

                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{user.Email}");
                LoggingController.LogDebug($"UserController.UpdateUserAsync - Cleaned up cache");
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"UserController.UpdateUserAsync - Error updating user {user.Email}: {ex.Message}");
                throw;
            }
        }

        public static async Task UpdateUserPublicityAsync(User user, bool newPublicity, CancellationToken token)
        {
            using var context = new HostingDbContext();

            try
            {
                token.ThrowIfCancellationRequested();
                user.IsPublic = newPublicity;

                context.Users.Update(user);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"UserController.UpdateUserPublicityAsync - Updated publicity for user {user.Email}");

                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{user.Email}");
                LoggingController.LogDebug($"UserController.UpdateUserAsync - Cleaned up cache");
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"UserController.UpdateUserPublicityAsync - Error updating publicity for user {user.Email}: {ex.Message}");
                throw;
            }
        }

        public static async Task UpdateUserFileDeletionTimeAsync(User user, TimeSpan new_deletion_time, CancellationToken token)
        {
            using var context = new HostingDbContext();

            try
            {
                token.ThrowIfCancellationRequested();
                user.AutoFileDeletionTime = new_deletion_time;

                context.Users.Update(user);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"UserController.UpdateUserFileDeletionTimeAsync - Updated file deletion time for user {user.Email}");

                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{user.Email}");
                LoggingController.LogDebug($"UserController.UpdateUserAsync - Cleaned up cache");
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"UserController.UpdateUserFileDeletionTimeAsync - Error updating file deletion time for user {user.Email}: {ex.Message}");
                throw;
            }
        }

        public static async Task DeleteUserAsync(User user, CancellationToken token)
        {
            using var context = new HostingDbContext();

            try
            {
                token.ThrowIfCancellationRequested();
                context.Users.Remove(user);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"UserController.DeleteUserAsync - Deleted user {user.Id} ({user.Email})");

                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{user.Email}");
                LoggingController.LogDebug($"UserController.UpdateUserAsync - Cleaned up cache");
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"UserController.DeleteUserAsync - Error deleting user {user.Id} ({user.Email}): {ex.Message}");
                throw;
            }
        }
    }
}