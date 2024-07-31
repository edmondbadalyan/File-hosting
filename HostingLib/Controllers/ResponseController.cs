using HostingLib.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPLib;

namespace HostingLib.Controllers
{
    public class ResponseController
    {
        public static async Task SendResponseAsync(TcpClient client, Response response)
        {
            string response_json = JsonConvert.SerializeObject(response);
            await TCP.SendString(client, response_json);

            //для файлов - пока не трогаем
            if (response.Payload != null && response.PayloadType == Payloads.FILE)
            {
                throw new NotImplementedException();
            }
        }

        public static async Task<Response> ReceiveResponseAsync(TcpClient client)
        {
            string received_json = await TCP.ReceiveString(client);
            return JsonConvert.DeserializeObject<Response>(received_json);
        }
    }
}
