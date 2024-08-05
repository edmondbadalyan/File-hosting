using HostingLib.Data.Context;
using HostingLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Controllers
{
    public class UserController
    {
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

            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);
            User user = new(email, encryption_controller.EncryptData(password), true, key, iv);

            context.Users.Add(user);
            await context.SaveChangesAsync();
            await context.DisposeAsync();

            Console.WriteLine($"User created successfully with email: {email} and password: {password} (encrypted - {user.Password}");
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
