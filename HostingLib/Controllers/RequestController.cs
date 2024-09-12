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
using System.Threading;
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
            { Requests.CLOSE_CONNECTION, new CloseConnectionHandler() },

            { Requests.GET_PUBLIC_KEY, new GetPublicKeyHandler() },
            { Requests.ENCRYPTED_DATA, new EncryptedDataHandler() },

            { Requests.USER_SPACE, new AvailableSpaceHandler() },
            { Requests.USER_AUTO_DELETE_TIME, new GetAutoDeletionTimeHandler() },
            { Requests.USER_CREATE, new CreateUserHandler() },
            { Requests.USER_GET, new GetUserHandler() },
            { Requests.USER_AUTHENTICATE, new AuthenticateUserHandler() },
            { Requests.USER_UPDATE, new UpdateUserHandler() },
            { Requests.USER_UPDATE_PUBLICITY, new UpdateUserPublicityHandler()},
            { Requests.USER_UPDATE_FILE_DELETION_TIME, new UpdateUserFileDeletionTimeHandler()},
            { Requests.USER_DELETE, new DeleteUserHandler() },

            { Requests.FILE_UPLOAD, new UploadFileHandler() },
            { Requests.FILE_DOWNLOAD, new DownloadFileHandler() },
            { Requests.FILE_GET, new GetFileHandler() },
            { Requests.FILE_GETALL, new GetAllFilesHandler() },
            { Requests.FILE_GET_PUBLIC, new GetPublicFilesHandler() },
            { Requests.FILE_GET_N, new GetFilesHandler() },
            { Requests.FILE_MOVE, new MoveFileHandler() },
            { Requests.FILE_UPDATE_PUBLICITY, new UpdateFilePublicityHandler() },
            { Requests.FILE_DELETE, new DeleteFileHandler() },
            { Requests.FILE_ERASE, new  EraseFileHandler()},

            { Requests.FOLDER_CREATE, new CreateFolderHandler() },
            { Requests.FOLDER_GET, new GetFolderHandler() },
            { Requests.FOLDER_MOVE, new MoveFolderHandler() },
            { Requests.FOLDER_UPDATE_PUBLICITY, new UpdateFolderPublicityHandler() },
            { Requests.FOLDER_DELETE, new DeleteFolderHandler() },
            { Requests.FOLDER_ERASE, new EraseFileHandler() },
        };

        public static async Task HandleClientAsync(TcpClient client)
        {
            ClientState state = new(client);
            client_states[client] = state;
            CancellationTokenSource cts = new();
            while (client.Connected || !cts.Token.IsCancellationRequested)
            {
                await ReceiveRequestAsync(state, cts.Token);
            }
            LoggingController.LogDebug($"Client closed connection");
            Console.WriteLine($"Client closed connection");
            client_states.Remove(client);
        }

        public static async Task SendRequestAsync(TcpClient client, Request request, CancellationToken token)
        {
            LoggingController.LogDebug($"RequestController.SendRequestAsync - Sent request {request.RequestType} with payload type {request.PayloadType} to {client.Client.RemoteEndPoint}");
            await TCP.SendString(client, JsonConvert.SerializeObject(request), token);
        }

        public static async Task ReceiveRequestAsync(ClientState state, CancellationToken token)
        {
            try
            {
                string received_json = await TCP.ReceiveString(state.Client);
                Console.WriteLine(received_json);
                LoggingController.LogDebug($"RequestControlller.ReceiveRequestAsync - Received {received_json}");

                Request request = JsonConvert.DeserializeObject<Request>(received_json);
                Response response = await HandleRequestAsync<Response>(state, request, token);

                if(state.Client.Connected)
                {
                    await ResponseController.SendResponseAsync(state.Client, response, token);
                }
                if (request.RequestType == Requests.CLOSE_CONNECTION)
                {
                    LoggingController.LogDebug($"Client {state.Client.Client.RemoteEndPoint} closed connection");
                    Console.WriteLine($"Client {state.Client.Client.RemoteEndPoint} closed connection");
                    state.Client.Close();
                }
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"RequestController.ReceiveRequestAsync - Threw exception {ex}");
                state.Client.Close();
            }
        }

        public static async Task<TResult> HandleRequestAsync<TResult>(ClientState state, Request request, CancellationToken token)
        {
            if (request_handlers.TryGetValue(request.RequestType, out var handler))
            {
                IRequestHandler<TResult> typed_handler = handler as IRequestHandler<TResult>;
                if(typed_handler != null)
                {
                    LoggingController.LogDebug($"RequestController.HandleRequestAsync - Handler found for request {request.RequestType.ToString()}");
                    return await typed_handler.HandleAsync(state, request, token);
                }
                else
                {
                    LoggingController.LogError($"RequestController.HandleRequestAsync - Handler for request type {request.RequestType.ToString()} is not of expected type.");
                    throw new InvalidOperationException($"Handler for request type {request.RequestType} is not of expected type.");
                }
            }
            else
            {
                LoggingController.LogError($"RequestController.HandleRequestAsync - No handler found for request type {request.RequestType.ToString()}");
                throw new InvalidOperationException($"No handler found for request type {request.RequestType}");
            }
        }

    }
}
