using HostingLib.Data.Context;
using HostingLib.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Controllers
{
    public class UserController
    {
        private readonly HostingDbContext context;
        private readonly EncryptionController encryption_controller;

        public UserController(HostingDbContext context)
        {
            this.context = context;
            this.encryption_controller = new();
        }

        public async Task CreateUser(string email, string password)
        {
            User user = new(email, encryption_controller.EncryptPassword(password), true);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            Console.WriteLine($"User created successfully with email: {email} and password: {password} (encrypted - {user.Password}");
        }
    }
}
