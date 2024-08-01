using Azure.Core;
using HostingLib.Classes;
using HostingLib.Handlers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPLib;
using Request = HostingLib.Classes.Request;

namespace HostingLib.Controllers
{
    public class RequestController
    {
        public static readonly Dictionary<Requests, object> request_handlers = new()
        {
            { Requests.GET_PUBLIC_KEY, new GetPublicKeyHandler() },
            { Requests.ENCRYPTED_DATA, new EncryptedDataHandler() },
            { Requests.USER_CREATE, new CreateUserHandler() },
            { Requests.USER_GET, new GetUserHandler() },
            { Requests.USER_UPDATE, new UpdateUserHandler() },
            { Requests.USER_AUTHORIZE, new AuthorizeUserHandler() }
        };

        public static async Task SendRequestAsync(TcpClient client, Request request)
        {
            await TCP.SendString(client, JsonConvert.SerializeObject(request));
        }

        public static async Task ReceiveRequestAsync(TcpClient client)
        {
            string received_json = await TCP.ReceiveString(client);
            Console.WriteLine(received_json);
            Request request = JsonConvert.DeserializeObject<Request>(received_json);
            Response response = await HandleRequestAsync<Response>(request);
            await ResponseController.SendResponseAsync(client, response);
        }

        public static async Task<TResult> HandleRequestAsync<TResult>(Request request)
        {
            if (request_handlers.TryGetValue(request.RequestType, out var handler))
            {
                var typed_handler = handler as IRequestHandler<TResult>;
                if(typed_handler != null)
                {
                    return await typed_handler.HandleAsync(request);
                }
                else
                {
                    throw new InvalidOperationException($"Handler for request type {request.RequestType} is not of expected type.");
                }
            }
            else
            {
                throw new InvalidOperationException($"No handler found for request type {request.RequestType}");
            }
        }

    }
}
