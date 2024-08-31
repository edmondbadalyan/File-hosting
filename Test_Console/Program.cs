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
            server = new("lunarhosting.ddns.net", 8080);

            string email, password;
            Console.WriteLine("Input email and password!");
            email = Console.ReadLine();
            password = Console.ReadLine();

            User received_user = await Client.GetUserAsync(server, email);
            if(received_user != null)
            {
                var list = await Client.GetAllFilesAsync(server, received_user);
                foreach( var file in list)
                {
                    Console.WriteLine($"{file.Name} {file.Size} {file.ChangeDate}");
                }
            }
            //else
            //{
            //    User user = await Client.AuthenticateUserAsync(server, received_user, password);
            //    Console.WriteLine($"Received and authenticated user {user.Id} {user.Email} {user.Password}");
            //}


            //else
            //{
            //    Console.WriteLine("Enter new password!");
            //    password = Console.ReadLine();
            //    await Client.UpdateUserAsync(server, received_user, password);
            //}

            //if (received_user != null)
            //{
            //    User user = await Client.AuthenticateUserAsync(server, received_user, password);
            //    if(user != null)
            //    {
            //        Console.WriteLine($"{user.Id} {user.Email} {user.Password}");
            //    }
            //    else
            //    {
            //        Console.WriteLine("Wrong password!");
            //    }
            //}
            //string file_path = @"C:\Users\Роман\Downloads\NQoZ_CwqyEM.jpg";
            //await Client.SendFileAsync(server, file_path, received_user.Id);

            //IList<HostingLib.Data.Entities.File> files = await Client.GetAllFilesAsync(server, received_user);

            //foreach(HostingLib.Data.Entities.File file in files)
            //{
            //    Console.WriteLine($"{file.Name} {file.Path}");
            //}
        }
    }
}
