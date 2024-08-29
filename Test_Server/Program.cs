using HostingLib.Controllers;
using HostingLib.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Test_Server;

string ip = ConfigManager.AppSetting["ip"];
int port = int.Parse(ConfigManager.AppSetting["port"]);

TcpListener listener = new(new IPEndPoint(IPAddress.Parse(ip), port));
listener.Start();

Console.WriteLine($"Waiting for connections at {ip}:{port}");

CancellationTokenSource cts = new();
Task deletion_task = Task.Run(async () => await FileDeletionHelper.RunAsync(cts.Token)); 

AppDomain.CurrentDomain.ProcessExit += (s, e) => cts.Cancel();

if (!Directory.Exists(FileController.storage_path))
{
    Directory.CreateDirectory(FileController.storage_path);
}

while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync();
    ListenToClient(client);
}

async void ListenToClient(TcpClient client)
{
    Console.WriteLine($"{client.Client.RemoteEndPoint} {DateTime.Now}");

    await RequestController.HandleClientAsync(client);
}
