using HostingLib.Data.Context;
using HostingLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostingLib.Controllers
{
    public class AuthorizationController
    {

        public static User Authenticate(User user, string given_password)
        {
            if (user is not null)
            {
                if (BCrypt.Net.BCrypt.Verify(given_password, user.Password))
                {
                    LoggingController.LogDebug($"AuthorizationController.Authenticate - User found: {user.Id} {user.Email}");
                    Console.WriteLine($"User found: {user.Id} {user.Email}");
                    return user;
                }
            }
            LoggingController.LogDebug($"AuthorizationController.Authenticate - User not found");
            return null;
        }

        public static async Task<File> Authorize(int user_id, int file_id)
        {
            using HostingDbContext context = new();
            return await context.Files.SingleOrDefaultAsync(f => f.Id == file_id && f.UserId == user_id && f.IsPublic && !f.IsDeleted);
        }
    }
}
