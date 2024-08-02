using Azure.Core;
using HostingLib.Classes;
using HostingLib.Controllers;
using HostingLib.Data.Context;
using HostingLib.Data.Entities;
using HostingLib.Helpers;
using HostingLib.Сlient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Request = HostingLib.Classes.Request;

namespace Test_Console
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            TcpClient server;
            server = new("192.168.0.12", 2024);

            string email, password;
            Console.WriteLine("Input email and password!");
            email = Console.ReadLine();
            password = Console.ReadLine();

            User received_user = await Client.GetUserAsync(server, email, password);
            //if(received_user == null)
            //{
            //    await Client.CreateUserAsync(server, email, password);
            //}
            //else
            //{
            //    Console.WriteLine($"{received_user.Id} {received_user.Email} {received_user.Password}");
            //}

            if (received_user != null)
            {
                Console.WriteLine("Enter new password!");
                password = Console.ReadLine();
                await Client.UpdateUserAsync(server, received_user, password);
            }


        }
    }
}
