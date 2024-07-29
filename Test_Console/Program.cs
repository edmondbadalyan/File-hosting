using HostingLib.Controllers;
using HostingLib.Data.Context;
using HostingLib.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Test_Console
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            HostingDbContext context = new();

            AuthorizationController authorization_controller = new(context);
            UserController user_controller = new(context);

            string email, password;
            Console.WriteLine("Input email and password!");
            email = Console.ReadLine();
            password = Console.ReadLine();

            User user = authorization_controller.Authenticate(email, password);
            if(user is null)
            {
                Console.WriteLine("User created!")
                await user_controller.CreateUser(email, password);
            }
            else
            {
                Console.WriteLine($"Welcome, {user.Email}!");
            }
        }
    }
}
