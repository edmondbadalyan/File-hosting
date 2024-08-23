using HostingLib.Data.Context;
using HostingLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Controllers
{
    public class UserController
    {
        private readonly static long user_quota = 16_106_127_360; //15 Гб

        public static async Task<long> GetAvailableSpace(int user_id)
        {
            HostingDbContext context = new();

            long used_space = await context.Files
                .Where(f => f.UserId == user_id)
                .SumAsync(f => f.Size);

            await context.DisposeAsync();

            return user_quota - used_space;
        }

        public static async Task<User> GetUser(string email)
        {
            using HostingDbContext context = new();
            User user = await context.Users.SingleOrDefaultAsync(u => u.Email == email);
            await context.DisposeAsync();
            return user;
        }

        public static async Task CreateUser(string email, string password)
        {
            using HostingDbContext context = new();

            User user = await GetUser(email);

            if(user != null)
            {
                throw new Exception("Such user already exists!");
            }
            else
            { 
                var (key, iv) = EncryptionController.GenerateKeyAndIv();
                EncryptionController encryption_controller = new(key, iv);
                user = new(email, encryption_controller.EncryptData(password), true, key, iv);

                context.Users
                    .Add(user);
                await context.SaveChangesAsync();
                await context.DisposeAsync();

                string user_directory = Path.Combine(FileController.storage_path, user.Id.ToString());
                Directory.CreateDirectory(user_directory);
                Directory.CreateDirectory(Path.Combine(user_directory, "Deleted"));

                Console.WriteLine($"User created successfully with email: {email} and password: {password} (encrypted - {user.Password}");
            }

        }

        public static async Task UpdateUser(User user, string new_password)
        {
            using HostingDbContext context = new();

            EncryptionController encryption_controller = new(user.EncryptionKey, user.Iv);
            string encrypted_password = encryption_controller.EncryptData(new_password);

            user.Password = encrypted_password;
            context.Users.
                Update(user);
            await context.SaveChangesAsync();
            await context.DisposeAsync();

            Console.WriteLine($"Updated user {user.Email}, new password is {new_password}");
        }

        public static async Task DeleteUser(User user)
        {
            using HostingDbContext context = new();

            context.Users
                .Remove(user);
            await context.SaveChangesAsync();
            await context.DisposeAsync();

            Console.WriteLine($"Deleted user {user.Id} {user.Email}");
        }
    }

}
