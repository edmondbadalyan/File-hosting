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

        public static async Task<User> Authenticate(User user, string given_password)
        {
            if (user is not null)
            {
                EncryptionController encryption_controller = new(user.EncryptionKey, user.Iv);
                if(encryption_controller.DecryptData(user.Password) == given_password)
                {
                    Console.WriteLine($"User found: {user.Id} {user.Email}");
                    return user;
                }
            }
            return null;
        }

        public static async Task<bool> Authorize(User user, int fileId)
        {
            using HostingDbContext context = new();
            bool isAuthorized = await context.User_Files.AnyAsync(uf => uf.User_id == user.Id && uf.File_id == fileId);
            return isAuthorized;
        }
    }
}
