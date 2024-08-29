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
        private static readonly long user_quota = 16_106_127_360; //15 Гб
        public static readonly string cache_prefix = "User:";

        public static async Task<long> GetAvailableSpace(int user_id, CancellationToken token)
        {
            string cache_key = $"{cache_prefix}space:{user_id}";

            long? cached_space = await CachedDataController.GetValueAsync<long>(cache_key);
            if (cached_space != null)
            {
                LoggingController.LogInfo($"UserController.AvailableSpace - Request for user {user_id} returned {cached_space} from cache");
                return (long)cached_space;
            }

            HostingDbContext context = new();

            try
            {
                token.ThrowIfCancellationRequested();

                long used_space = await context.Files
                    .Where(f => f.UserId == user_id)
                    .SumAsync(f => f.Size, token);


                LoggingController.LogInfo($"UserController.AvailableSpace - Request for user {user_id} returned {user_quota - used_space}");
                await CachedDataController.SetValueAsync(cache_key, user_quota - used_space);
                return user_quota - used_space;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task<User> GetUser(string email, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();
                User user = await context.Users.SingleOrDefaultAsync(u => u.Email == email, token);
                LoggingController.LogInfo($"UserController.GetUser - request with email {email} returned {(user?.Id)}");
                return user;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task CreateUser(string email, string password, bool isPublic, CancellationToken token)
        {
            using HostingDbContext context = new();

            try
            {
                token.ThrowIfCancellationRequested();

                User user = await GetUser(email, token);

                if(user != null)
                {
                    LoggingController.LogError($"UserController.CreateUser - error: user with the email {email} already exists");
                    throw new Exception("Such user already exists!");
                }
                else
                { 
                    user = new(email, BCrypt.Net.BCrypt.HashPassword(password), true, isPublic);

                    token.ThrowIfCancellationRequested();

                    context.Users
                        .Add(user);
                    await context.SaveChangesAsync(token);

                    string user_directory = Path.Combine(FileController.storage_path, user.Id.ToString());
                    Directory.CreateDirectory(user_directory);
                    Directory.CreateDirectory(Path.Combine(user_directory, "Deleted"));

                    LoggingController.LogInfo($"UserController.CreateUser - successfully created user with email: {email}");
                    Console.WriteLine($"User created successfully with email: {email}");
                }
            }
            finally
            {
                await context.DisposeAsync();
            }


        }

        public static async Task UpdateUser(User user, string new_password, CancellationToken token)
        {
            using HostingDbContext context = new();

            try
            {
                string encrypted_password = BCrypt.Net.BCrypt.HashPassword(new_password);

                token.ThrowIfCancellationRequested();

                user.Password = encrypted_password;
                context.Users.
                    Update(user);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"UserController.UpdateUser - updated user {user.Email}");
                Console.WriteLine($"Updated user {user.Email}");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task UpdateUserPublicity(User user, bool new_publicity, CancellationToken token)
        {
            using HostingDbContext context = new();

            try
            {

                token.ThrowIfCancellationRequested();

                user.IsPublic = new_publicity;
                context.Users.
                    Update(user);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"UserController.UpdateUserPublicity - updated user publicity {user.Email}");
                Console.WriteLine($"Updated user publicty {user.Email}");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task DeleteUser(User user, CancellationToken token)
        {
            using HostingDbContext context = new();

            try
            {
                token.ThrowIfCancellationRequested();

                context.Users
                    .Remove(user);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"UserController.DeleteUser - deleted user {user.Id} {user.Email}");
                Console.WriteLine($"Deleted user {user.Id} {user.Email}");
            }
            finally
            {
                await context.DisposeAsync();
            }

        }
    }

}
