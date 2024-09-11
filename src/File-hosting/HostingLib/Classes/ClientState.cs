using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Classes
{
    public class ClientState
    {
        public TcpClient Client { get; }
        public NetworkStream Stream => Client.GetStream();

        public ClientState(TcpClient client)
        {
            Client = client;
        }
    }
}
