using HostingLib.Controllers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Test_Server;

string ip = ConfigManager.AppSetting["ip"];
int port = int.Parse(ConfigManager.AppSetting["port"]);

TcpListener listener = new(new IPEndPoint(IPAddress.Parse(ip), port));
listener.Start();

Console.WriteLine($"Waiting for connections at {ip}:{port}");

while(true)
{
    TcpClient client = await listener.AcceptTcpClientAsync();
    ListenToClient(client);
}

async void ListenToClient(TcpClient client)
{
    Console.WriteLine($"{client.Client.RemoteEndPoint} {DateTime.Now}");
    while (true)
    {
        await RequestController.HandleClientAsync(client);
    }
}