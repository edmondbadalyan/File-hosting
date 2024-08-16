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
        private static readonly Dictionary<TcpClient, ClientState> client_states = new();

        public static readonly Dictionary<Requests, IRequestHandler<Response>> request_handlers = new()
        {
            { Requests.GET_PUBLIC_KEY, new GetPublicKeyHandler() },
            { Requests.ENCRYPTED_DATA, new EncryptedDataHandler() },
            
            { Requests.USER_CREATE, new CreateUserHandler() },
            { Requests.USER_GET, new GetUserHandler() },
            { Requests.USER_AUTHENTICATE, new AuthenticateUserHandler() },
            { Requests.USER_UPDATE, new UpdateUserHandler() },
            { Requests.USER_DELETE, new DeleteUserHandler() },

            { Requests.FILE_UPLOAD, new UploadFileHandler() },
            { Requests.FILE_DOWNLOAD, new DownloadFileHandler() },
            { Requests.FILE_GETALL, new GetAllFilesHandler() },
            { Requests.FILE_GET, new GetFileHandler() },
            { Requests.FILE_MOVE, new MoveFileHandler() },
            { Requests.FILE_DELETE, new DeleteFileHandler() },
            { Requests.FILE_ERASE, new  EraseFileHandler()},

            { Requests.FOLDER_CREATE, new CreateFolderHandler() },
            { Requests.FOLDER_GET, new GetFolderHandler() },
            { Requests.FOLDER_MOVE, new MoveFolderHandler() },
            { Requests.FOLDER_DELETE, new DeleteFolderHandler() },
            { Requests.FOLDER_ERASE, new EraseFileHandler() },
        };

        public static async Task HandleClientAsync(TcpClient client)
        {
            ClientState state = new(client);
            client_states[client] = state;
            await ReceiveRequestAsync(state);
        }

        public static async Task SendRequestAsync(TcpClient client, Request request)
        {
            await TCP.SendString(client, JsonConvert.SerializeObject(request));
        }

        public static async Task ReceiveRequestAsync(ClientState state)
        {
            string received_json = await TCP.ReceiveString(state.Client);
            Console.WriteLine(received_json);
            Request request = JsonConvert.DeserializeObject<Request>(received_json);
            Response response = await HandleRequestAsync<Response>(state, request);
            await ResponseController.SendResponseAsync(state.Client, response);
        }

        public static async Task<TResult> HandleRequestAsync<TResult>(ClientState state, Request request)
        {
            if (request_handlers.TryGetValue(request.RequestType, out var handler))
            {
                var typed_handler = handler as IRequestHandler<TResult>;
                if(typed_handler != null)
                {
                    return await typed_handler.HandleAsync(state, request);
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
