using Azure.Core;
using HostingLib.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Request = HostingLib.Classes.Request;
using TCPLib;
using Newtonsoft.Json;
using HostingLib.Controllers;
using System.Threading;
using HostingLib.Сlient;
using StackExchange.Redis;

namespace HostingLib.Helpers
{
    public class ClientEncryptionHelper
    {
        private readonly RSACryptoServiceProvider rsa;
        private readonly byte[] key;
        private readonly byte[] iv;

        public ClientEncryptionHelper(TcpClient client, byte[] _key, byte[] _iv, CancellationToken token)
        { 
            rsa = new RSACryptoServiceProvider();
            string public_key = GetServerPublicKeyAsync(client, token).Result;
            rsa.FromXmlString(public_key);
            key = _key;
            iv = _iv;
        }

        private async Task<string> GetServerPublicKeyAsync(TcpClient server, CancellationToken token)
        {
            Request request = new(Requests.GET_PUBLIC_KEY, Payloads.PUBLIC_KEY, "");
            await RequestController.SendRequestAsync(server, request, token);
            Response response = await ResponseController.ReceiveResponseAsync(server, token);
            return response.Payload;
        }

        public async Task<Response> ExchangeEncryptedDataAsync(TcpClient server, Request request_to_send, CancellationToken token)
        {
            byte[] encryptedKey = rsa.Encrypt(key, false);
            byte[] encryptedIv = rsa.Encrypt(iv, false);

            EncryptedDataPayload payload = new(encryptedKey, encryptedIv, JsonConvert.SerializeObject(request_to_send));

            Request request = new(Requests.ENCRYPTED_DATA, Payloads.DATA, JsonConvert.SerializeObject(payload));
            await RequestController.SendRequestAsync(server, request, token);
            Response response = await ResponseController.ReceiveResponseAsync(server, token);
            return response;
        }
    }
}
