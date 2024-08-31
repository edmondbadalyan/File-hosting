using HostingLib.Classes;
using HostingLib.Controllers;
using HostingLib.Data.Entities;
using HostingLib.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostingLib.Handlers
{
    public interface IRequestHandler<TResult>
    {
        Task<TResult> HandleAsync(ClientState state, Classes.Request request, CancellationToken token);
    }

    public class CloseConnectionHandler : IRequestHandler<Response>
    {
        public Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                return Task.FromResult(new Response(Responses.Success, Payloads.MESSAGE, "The connection will be closed shortly"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new Response(Responses.Fail, Payloads.MESSAGE, ex.Message));
            }
        }
    }

    public class GetPublicKeyHandler : IRequestHandler<Response>
    {
        public Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                string public_key = ServerEncryptionHelper.GetPublicKey();
                return Task.FromResult(new Response(Responses.Success, Payloads.PUBLIC_KEY, public_key));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new Response(Responses.Fail, Payloads.MESSAGE, ex.Message));
            }
        }
    }

    public class EncryptedDataHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                EncryptedDataPayload payload = JsonConvert.DeserializeObject<EncryptedDataPayload>(request.Payload);
                byte[] key = ServerEncryptionHelper.Decrypt(payload.Key);
                byte[] iv = ServerEncryptionHelper.Decrypt(payload.Iv);
                EncryptionController encryption_controller = new(key, iv);

                Request appended_request = JsonConvert.DeserializeObject<Request>(payload.AppendedRequest);
                dynamic appended_request_payload = null;
                Request decrypted_request = null;

                switch (appended_request.PayloadType)
                {
                    case Payloads.USER:
                        {
                            string user = null;
                            string email = null, password = null; 
                            bool isPublic = false;

                            appended_request_payload = JsonConvert.DeserializeObject<UserPayload>(appended_request.Payload);

                            if (appended_request_payload.User != null)
                            {
                                user = encryption_controller.DecryptData(appended_request_payload.User);
                            }

                            if (appended_request_payload.Email != null)
                            {
                                email = encryption_controller.DecryptData(appended_request_payload.Email);
                            }

                            if (appended_request_payload.Password != null)
                            {
                                password = encryption_controller.DecryptData(appended_request_payload.Password);
                            }

                            if(appended_request_payload.IsPublic != null)
                            {
                                isPublic = appended_request_payload.IsPublic;
                            }

                            appended_request_payload = new UserPayload(user, email, password, isPublic);

                            string decrypted_payload = JsonConvert.SerializeObject(appended_request_payload);
                            decrypted_request = new(appended_request.RequestType, Payloads.USER, decrypted_payload);
                            break;
                        }
                    case Payloads.FILE:
                        {
                            string file = null, file_name = null;
                            string file_info = null; 
                            bool isPublic = false;
                            int user_id = 0;
                            string parent_id = null;

                            appended_request_payload = JsonConvert.DeserializeObject<FilePayload>(appended_request.Payload);

                            if (appended_request_payload.File != null)
                            {
                                file = encryption_controller.DecryptData(appended_request_payload.File);
                            }

                            if (appended_request_payload.FileName  != null)
                            {
                                file_name = encryption_controller.DecryptData(appended_request_payload.FileName);
                            }

                            if (appended_request_payload.FileDetails != null)
                            {
                                file_info = encryption_controller.DecryptData(appended_request_payload.FileDetails);
                            }

                            if(appended_request_payload.IsPublic != null)
                            {
                                isPublic = appended_request_payload.IsPublic;
                            }

                            user_id = appended_request_payload.UserId;
                            parent_id = appended_request_payload.ParentId;

                            appended_request_payload = new FilePayload(file, file_name, file_info, isPublic, user_id, parent_id);

                            string new_payload = JsonConvert.SerializeObject(appended_request_payload);

                            decrypted_request = new(appended_request.RequestType, Payloads.FILE, new_payload);
                            break;
                        }
                    case Payloads.FOLDER:
                        {
                            string folder = null, folder_name = null, folder_path = null;
                            bool isPublic = false;
                            int user_id = 0;
                            string parent_id = null;

                            appended_request_payload = JsonConvert.DeserializeObject<FilePayload>(appended_request.Payload);

                            if(appended_request_payload.Folder != null)
                            {
                                folder = encryption_controller.DecryptData(appended_request_payload.Folder);
                            }
                            if(appended_request_payload.FolderName != null)
                            {
                                folder_name = encryption_controller.DecryptData(appended_request_payload.FolderName);
                            }
                            if(appended_request_payload.FolderPath != null)
                            {
                                folder_path = encryption_controller.DecryptData(appended_request_payload.FolderPath);
                            }
                            if(appended_request_payload.IsPublic != null)
                            {
                                isPublic = appended_request_payload.IsPublic;
                            }

                            user_id = appended_request_payload.UserId;
                            parent_id = appended_request_payload.ParentId;

                            appended_request_payload = new FolderPayload(folder, folder_name, folder_path, isPublic, user_id, parent_id);

                            string new_payload = JsonConvert.SerializeObject(appended_request_payload);

                            decrypted_request = new(appended_request.RequestType, Payloads.FOLDER, new_payload);
                            break;
                        }
                }

                Response response = await RequestController.HandleRequestAsync<Response>(state, decrypted_request, token);
                return response;
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    #region User

    public class AvailableSpaceHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                UserPayload payload = JsonConvert.DeserializeObject<UserPayload>(request.Payload);
                User user = JsonConvert.DeserializeObject<User>(payload.User);
                long space = await UserController.GetAvailableSpace(user.Id, token);
                return new(Responses.Success, Payloads.USER, space.ToString());
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class CreateUserHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                UserPayload payload = JsonConvert.DeserializeObject<UserPayload>(request.Payload);
                await UserController.CreateUser(payload.Email, payload.Password, payload.IsPublic, token);
                return new(Responses.Success, Payloads.MESSAGE, $"User created successfully with email {payload.Email}");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class GetUserHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                UserPayload payload = JsonConvert.DeserializeObject<UserPayload>(request.Payload);
                User user = await UserController.GetUser(payload.Email, token);
                return new(Responses.Success, Payloads.USER, JsonConvert.SerializeObject(user));
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class AuthenticateUserHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                UserPayload payload = JsonConvert.DeserializeObject<UserPayload>(request.Payload);
                User user = AuthorizationController.Authenticate(JsonConvert.DeserializeObject<User>(payload.User), payload.Password);
                return new(Responses.Success, Payloads.USER, JsonConvert.SerializeObject(user));
            }

            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class UpdateUserHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                UserPayload payload = JsonConvert.DeserializeObject<UserPayload>(request.Payload);
                User user = JsonConvert.DeserializeObject<User>(payload.User);
                await UserController.UpdateUser(user, payload.Password, token);
                return new(Responses.Success, Payloads.MESSAGE, "User updated successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class UpdateUserPublicityHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                UserPayload payload = JsonConvert.DeserializeObject<UserPayload>(request.Payload);
                User user = JsonConvert.DeserializeObject<User>(payload.User);
                await UserController.UpdateUserPublicity(user, payload.IsPublic, token);
                return new(Responses.Success, Payloads.MESSAGE, "User publicity updated successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class DeleteUserHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                UserPayload payload = JsonConvert.DeserializeObject<UserPayload>(request.Payload);
                User user = JsonConvert.DeserializeObject<User>(payload.User);
                await UserController.DeleteUser(user, token);
                return new(Responses.Success, Payloads.MESSAGE, "User deleted successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }
    #endregion

    #region File

    public class UploadFileHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);
                FileDetails info = JsonConvert.DeserializeObject<FileDetails>(payload.FileDetails);
                string file_path = Path.Combine(FileController.storage_path, payload.UserId.ToString(), info.Name);

                if (System.IO.File.Exists(file_path))
                {
                    LoggingController.LogError($"UploadFileHandler.HandleAsync - Error: file {Path.GetFileName(file_path)} already exists at {file_path}");
                    throw new InvalidOperationException($"File {Path.GetFileName(file_path)} already exists at {file_path}");
                }

                Response response = new(Responses.Success, Payloads.MESSAGE, "Ready to receive file");
                await ResponseController.SendResponseAsync(state.Client, response, token);

                try
                {
                    await FileController.DownloadFileAsync(state.Client, file_path, payload.UserId, payload.ParentId != null ? int.Parse(payload.ParentId) : null, token);
                }
                catch (Exception ex) when (ex is IOException || ex is SocketException)
                {
                    LoggingController.LogError($"UploadFileHandler.HandleAsync - Connection lost during file upload: {ex.Message}");
                    return new Response(Responses.Fail, Payloads.MESSAGE, "Connection was lost during file upload.");
                }

                await FileController.CreateFile(info, payload.UserId, payload.ParentId != null ? int.Parse(payload.ParentId) : null, payload.IsPublic, token);

                return new Response(Responses.Success, Payloads.MESSAGE, "File uploaded successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }


    public class DownloadFileHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);
                Data.Entities.File file = JsonConvert.DeserializeObject<Data.Entities.File>(payload.File);
                string file_path = Path.Combine(FileController.storage_path, payload.UserId.ToString(), file.Name);

                if(file.UserId != payload.UserId && !file.IsPublic)
                {
                    LoggingController.LogError($"FileController.DownloadFileAsync - Access denied for file {file.Path} to user {payload.UserId}");
                    throw new UnauthorizedAccessException("Access denied. The file is not public.");
                }

                Response response = new Response(Responses.Success, Payloads.MESSAGE, "Ready to send file");
                await ResponseController.SendResponseAsync(state.Client, response, token);

                try
                {
                    await FileController.UploadFileAsync(state.Client, file_path, token);
                }
                catch (Exception ex) when (ex is IOException || ex is SocketException)
                {
                    LoggingController.LogError($"DownloadFileHandler.HandleAsync - Connection lost during file upload: {ex.Message}");
                    return new Response(Responses.Fail, Payloads.MESSAGE, "Connection was lost during file upload.");
                }

                return new(Responses.Success, Payloads.MESSAGE, "File downloaded successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class GetFileHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);
                Data.Entities.File file = await FileController.GetFile(payload.FileName, payload.UserId, token);

                return new(Responses.Success, Payloads.FILE, JsonConvert.SerializeObject(file));
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class GetAllFilesHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);

                IList<Data.Entities.File> files = await FileController.GetAllFiles(payload.UserId, token);

                return new(Responses.Success, Payloads.FILE, JsonConvert.SerializeObject(files));
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class GetPublicFilesHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);

                IList<Data.Entities.File> files = await FileController.GetPublicFiles(payload.UserId, token);

                Console.WriteLine($"Found {files.Count} public files for user {payload.UserId}");

                return new(Responses.Success, Payloads.FILE, JsonConvert.SerializeObject(files));
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class GetFilesHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);

                IList<Data.Entities.File> files = await FileController.GetFiles(payload.UserId, payload.ParentId != null ? int.Parse(payload.ParentId) : null, token);

                Console.WriteLine($"Found {files.Count} files for user {payload.UserId}");

                return new(Responses.Success, Payloads.FILE, JsonConvert.SerializeObject(files));
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }


    public class MoveFileHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);
                Data.Entities.File file = JsonConvert.DeserializeObject<Data.Entities.File>(payload.File);
                await FileController.MoveFile(file, payload.FileName, token);

                return new(Responses.Success, Payloads.MESSAGE, "File moved successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class UpdateFilePublicityHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);
                Data.Entities.File file = JsonConvert.DeserializeObject<Data.Entities.File>(payload.File);
                await FileController.UpdateFilePublicity(file, payload.IsPublic, token);

                return new(Responses.Success, Payloads.MESSAGE, "File publicity updated successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class DeleteFileHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);
                Data.Entities.File file = JsonConvert.DeserializeObject<Data.Entities.File>(payload.File);
                await FileController.DeleteFile(file, token);

                return new(Responses.Success, Payloads.MESSAGE, "File deleted successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class EraseFileHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);
                Data.Entities.File file = JsonConvert.DeserializeObject<Data.Entities.File>(payload.File);
                await FileController.EraseFile(file, token);

                return new(Responses.Success, Payloads.MESSAGE, "File erased successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }


    #endregion

    #region Folder

    public class CreateFolderHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FolderPayload payload = JsonConvert.DeserializeObject<FolderPayload>(request.Payload);
                int? parent_id = JsonConvert.DeserializeObject<int?>(payload.ParentId);
                await FileController.CreateFolder(payload.FolderName, payload.UserId, parent_id, payload.IsPublic, token);

                return new(Responses.Success, Payloads.MESSAGE, "Folder created successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class GetFolderHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FolderPayload payload = JsonConvert.DeserializeObject<FolderPayload>(request.Payload);
                Data.Entities.File folder = await FileController.GetFolder(payload.FolderName, token);

                return new(Responses.Success, Payloads.FOLDER, JsonConvert.SerializeObject(folder));
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class MoveFolderHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FolderPayload payload = JsonConvert.DeserializeObject<FolderPayload>(request.Payload);
                Data.Entities.File folder = JsonConvert.DeserializeObject<Data.Entities.File>(payload.Folder);
                await FileController.MoveFolder(folder, payload.FolderPath, token);

                return new(Responses.Success, Payloads.MESSAGE, "Folder moved successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class UpdateFolderPublicityHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FolderPayload payload = JsonConvert.DeserializeObject<FolderPayload>(request.Payload);
                Data.Entities.File file = JsonConvert.DeserializeObject<Data.Entities.File>(payload.Folder);
                await FileController.UpdateFolderPublicity(file, payload.IsPublic, token);

                return new(Responses.Success, Payloads.MESSAGE, "Folder publicity updated successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class DeleteFolderHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FolderPayload payload = JsonConvert.DeserializeObject<FolderPayload>(request.Payload);
                Data.Entities.File folder = JsonConvert.DeserializeObject<Data.Entities.File>(payload.Folder);
                await FileController.DeleteFolder(folder, token);

                return new(Responses.Success, Payloads.MESSAGE, "Folder moved to deleted successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class EraseFolderHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request, CancellationToken token)
        {
            try
            {
                FolderPayload payload = JsonConvert.DeserializeObject<FolderPayload>(request.Payload);
                Data.Entities.File folder = JsonConvert.DeserializeObject<Data.Entities.File>(payload.Folder);
                await FileController.EraseFolder(folder, token);

                return new(Responses.Success, Payloads.MESSAGE, "Folder erased successfully!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, "Operation was canceled");
            }
            catch (Exception ex)
            {
                return new Response(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }

        }

    }

    #endregion
}
