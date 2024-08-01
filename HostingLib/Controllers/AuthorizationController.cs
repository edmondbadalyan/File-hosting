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
    public class AuthorizationController
    {
        private readonly HostingDbContext context;
        private readonly EncryptionController encryption_controller;

        public AuthorizationController(HostingDbContext context)
        {
            this.context = context;
            encryption_controller = new EncryptionController();
        }

        public User Authenticate(string email, string password)
        {
            User user = context.Users.SingleOrDefault(u => u.Email == email);
            if (user is not null)
            {
                if(encryption_controller.DecryptPassword(user.Password) == password)
                {
                    Console.WriteLine($"User found: {email}");
                    return user;
                }
            }
            return null;
        }

        public bool Authorize(User user, int fileId)
        {
            return context.User_Files.Any(uf => uf.User_id == user.Id && uf.File_id == fileId);
        }
    }
}
