using HostingLib.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPLib;

namespace HostingLib.Controllers
{
    public class ResponseController
    {
        public static async Task SendResponseAsync(TcpClient client, Response response, CancellationToken token)
        {
            string response_json = JsonConvert.SerializeObject(response);
            await TCP.SendString(client, response_json, token);
            LoggingController.LogDebug($"ResponseController.SendResponseAsync - Sent response {response.ResponseType.ToString()} with payload type {response.PayloadType.ToString()}");
                
        }

        public static async Task<Response> ReceiveResponseAsync(TcpClient client, CancellationToken token)
        {
            string received_json = await TCP.ReceiveString(client, token);
            LoggingController.LogDebug($"ResponseController.ReceiveResponseAsync - Received {received_json}");
            return JsonConvert.DeserializeObject<Response>(received_json);
        }
    }
}
