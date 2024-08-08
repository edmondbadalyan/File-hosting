using HostingLib.Classes;
using HostingLib.Controllers;
using HostingLib.Data.Entities;
using HostingLib.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Handlers
{
    public interface IRequestHandler<TResult>
    {
        Task<TResult> HandleAsync(ClientState state, Classes.Request request);
    }

    public class GetPublicKeyHandler : IRequestHandler<Response>
    {
        public Task<Response> HandleAsync(ClientState state, Request request)
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
        public async Task<Response> HandleAsync(ClientState state, Request request)
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

                            appended_request_payload = JsonConvert.DeserializeObject<UserPayload>(appended_request.Payload);

                            if (appended_request_payload.User != null)
                            {
                                user = appended_request_payload.User;
                            }

                            if (appended_request_payload.Email != null)
                            {
                                email = encryption_controller.DecryptData(appended_request_payload.Email);
                            }

                            if (appended_request_payload.Password != null)
                            {
                                password = encryption_controller.DecryptData(appended_request_payload.Password);
                            }
                            appended_request_payload = new UserPayload(user, email, password);

                            string decrypted_payload = JsonConvert.SerializeObject(appended_request_payload);
                            decrypted_request = new(appended_request.RequestType, Payloads.USER, decrypted_payload);
                            break;
                        }
                    case Payloads.FILE:
                        {
                            string file = null;
                            string file_info = null;
                            int user_id = 0;

                            appended_request_payload = JsonConvert.DeserializeObject<FilePayload>(appended_request.Payload);

                            if (appended_request_payload.File != null)
                            {
                                file = appended_request_payload.File;
                            }

                            if (appended_request_payload.FileDetails != null)
                            {
                                file_info = appended_request_payload.FileDetails;
                            }

                            user_id = appended_request_payload.UserId;

                            appended_request_payload = new FilePayload(file, file_info, user_id);

                            string new_payload = JsonConvert.SerializeObject(appended_request_payload);

                            decrypted_request = new(appended_request.RequestType, Payloads.FILE, new_payload);
                            break;
                        }
                }

                Response response = await RequestController.HandleRequestAsync<Response>(state, decrypted_request);
                return response;
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    #region User

    public class CreateUserHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request)
        {
            try
            {
                UserPayload payload = JsonConvert.DeserializeObject<UserPayload>(request.Payload);
                await UserController.CreateUser(payload.Email, payload.Password);
                return new(Responses.Success, Payloads.MESSAGE, $"User created successfully with email {payload.Email} and pass {payload.Password}");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class GetUserHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request)
        {
            try
            {
                UserPayload payload = JsonConvert.DeserializeObject<UserPayload>(request.Payload);
                User user = await UserController.GetUser(payload.Email);
                return new(Responses.Success, Payloads.USER, JsonConvert.SerializeObject(user));
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class AuthenticateUserHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request)
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
        public async Task<Response> HandleAsync(ClientState state, Request request)
        {
            try
            {
                UserPayload payload = JsonConvert.DeserializeObject<UserPayload>(request.Payload);
                User user = JsonConvert.DeserializeObject<User>(payload.User);
                await UserController.UpdateUser(user, payload.Password);
                return new(Responses.Success, Payloads.MESSAGE, "User updated successfully!");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class DeleteUserHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request)
        {
            try
            {
                UserPayload payload = JsonConvert.DeserializeObject<UserPayload>(request.Payload);
                User user = JsonConvert.DeserializeObject<User>(payload.User);
                await UserController.DeleteUser(user);
                return new(Responses.Success, Payloads.MESSAGE, "User deleted successfully!");
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
        public async Task<Response> HandleAsync(ClientState state, Request request)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);
                FileDetails info = JsonConvert.DeserializeObject<FileDetails>(payload.FileDetails);
                string file_path = Path.Combine(FileController.storage_path, payload.UserId.ToString(), info.Name);

                Response response = new Response(Responses.Success, Payloads.MESSAGE, "Ready to receive file");
                await ResponseController.SendResponseAsync(state.Client, response);

                await FileController.DownloadFileAsync(state.Client, file_path);
                await FileController.CreateFile(info, payload.UserId);

                return new(Responses.Success, Payloads.MESSAGE, "File uploaded successfully!");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class DownloadFileHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);
                Data.Entities.File file = JsonConvert.DeserializeObject<Data.Entities.File>(payload.File);
                string file_path = Path.Combine(FileController.storage_path, payload.UserId.ToString(), file.Name);

                Response response = new Response(Responses.Success, Payloads.MESSAGE, "Ready to send file");
                await ResponseController.SendResponseAsync(state.Client, response);

                await FileController.UploadFileAsync(state.Client, file_path);

                return new(Responses.Success, Payloads.MESSAGE, "File downloaded successfully!");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class GetAllFilesHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);

                IList<Data.Entities.File> files = await FileController.GetAllFiles(payload.UserId);

                return new(Responses.Success, Payloads.FILE, JsonConvert.SerializeObject(files));
            }
            catch(Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class GetFileHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);
                Data.Entities.File file_info = JsonConvert.DeserializeObject<Data.Entities.File>(payload.File);
                Data.Entities.File file = await FileController.GetFile(file_info.Name, payload.UserId);

                return new(Responses.Success, Payloads.FILE, JsonConvert.SerializeObject(file));
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class DeleteFileHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(ClientState state, Request request)
        {
            try
            {
                FilePayload payload = JsonConvert.DeserializeObject<FilePayload>(request.Payload);
                Data.Entities.File file = JsonConvert.DeserializeObject<Data.Entities.File>(payload.File);
                await FileController.DeleteFile(file);

                return new(Responses.Success, Payloads.MESSAGE, "File deleted successfully!");
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    #endregion
}
