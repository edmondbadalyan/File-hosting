using HostingLib.Classes;
using HostingLib.Controllers;
using HostingLib.Data.Entities;
using HostingLib.Helpers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostingLib.Сlient
{
    public class Client
    {
        public static async Task<bool> CloseConnectionAsync(TcpClient server)
        {
            CancellationTokenSource cts = new();

            try
            {
                Request request = new(Requests.CLOSE_CONNECTION, Payloads.MESSAGE, "");
                await RequestController.SendRequestAsync(server, request, cts.Token);

                Response response = await ResponseController.ReceiveResponseAsync(server, cts.Token);

                return response.ResponseType == Responses.Success;
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        #region User

        public static async Task<long> GetAvailableSpaceAsync(TcpClient server, User user)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                UserPayload appended_request_payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(user)), null, null, false, null);
                Request appended_request = new(Requests.USER_SPACE, Payloads.USER, JsonConvert.SerializeObject(appended_request_payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, appended_request, cts.Token);

                return JsonConvert.DeserializeObject<long>(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task<TimeSpan> GetAutoDeletionTimeAsync(TcpClient server, User user)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                UserPayload appended_request_payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(user)), null, null, false, null);
                Request appended_request = new(Requests.USER_AUTO_DELETE_TIME, Payloads.USER, JsonConvert.SerializeObject(appended_request_payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, appended_request, cts.Token);

                return TimeSpan.Parse(response.Payload);
                //return JsonConvert.DeserializeObject<TimeSpan>(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task<User> GetUserAsync(TcpClient server, string email) 
        {
            CancellationTokenSource cts = new();

            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                string encrypted_email = encryption_controller.EncryptData(email);

                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                UserPayload appended_request_payload = new(null, encrypted_email, null, false, null);
                Request appended_request = new(Requests.USER_GET, Payloads.USER, JsonConvert.SerializeObject(appended_request_payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, appended_request, cts.Token);

                return JsonConvert.DeserializeObject<User>(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }

        }

        public static async Task<User> AuthenticateUserAsync(TcpClient server, User user, string given_password)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                string encrypted_password = encryption_controller.EncryptData(given_password);

                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                UserPayload request_payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(user)), null, encrypted_password, false, null);
                Request request = new(Requests.USER_AUTHENTICATE, Payloads.USER, JsonConvert.SerializeObject(request_payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                return JsonConvert.DeserializeObject<User>(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task CreateUserAsync(TcpClient server, string email, string password, bool isPublic, TimeSpan? auto_file_deletion_time)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);
           
            try
            {
                string encrypted_email = encryption_controller.EncryptData(email);
                string encrypted_password = encryption_controller.EncryptData(password);

                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                UserPayload appended_request_payload = new(null, encrypted_email, encrypted_password, isPublic, auto_file_deletion_time.ToString());
                Request appended_request = new(Requests.USER_CREATE, Payloads.USER, JsonConvert.SerializeObject(appended_request_payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, appended_request, cts.Token);
                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }

        }

        public static async Task UpdateUserAsync(TcpClient server, User user, string password)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                string encrypted_password = encryption_controller.EncryptData(password);

                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                UserPayload request_payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(user)), null, encrypted_password, false, null);
                Request request = new(Requests.USER_UPDATE, Payloads.USER, JsonConvert.SerializeObject(request_payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);
                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task UpdateUserPublicityAsync(TcpClient server, User user, bool publicity)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                UserPayload request_payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(user)), null, null, publicity, null);
                Request request = new(Requests.USER_UPDATE_PUBLICITY, Payloads.USER, JsonConvert.SerializeObject(request_payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);
                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task UpdateUserFileDeletionTimeAsync(TcpClient server, User user, TimeSpan file_deletion_time)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                UserPayload request_payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(user)), null, null, false, file_deletion_time.ToString());
                Request request = new(Requests.USER_UPDATE_FILE_DELETION_TIME, Payloads.USER, JsonConvert.SerializeObject(request_payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);
                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task DeleteUserAsync(TcpClient server, User user)
        {
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);
            CancellationTokenSource cts = new();
            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                UserPayload request_payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(user)), null, null, false, null);
                Request request = new(Requests.USER_DELETE, Payloads.USER, JsonConvert.SerializeObject(request_payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);
                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }
        #endregion

        #region File

        public static async Task<Response> UploadFileAsync(TcpClient server, string from_file_path, User user, Data.Entities.File? parent, bool isPublic, IProgress<double> progress = null)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FileInfo info = new(from_file_path);
                FileDetails details = new(info.Name, info.Length, info.Extension, info.CreationTime, info.LastWriteTime);

                if (await GetAvailableSpaceAsync(server, user) < info.Length)
                {
                    return new Response(Responses.Fail, Payloads.MESSAGE, "The file exceeds the available user quota!");
                }
            
                FilePayload payload = new(null, null, encryption_controller.EncryptData(JsonConvert.SerializeObject(details)), isPublic, user.Id, parent?.Id.ToString());
                Request request = new(Requests.FILE_UPLOAD, Payloads.FILE, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                if(response.ResponseType != Responses.Success)
                {
                    return response;
                }

                var progress_handler = new Progress<double>(value =>
                {
                    progress?.Report(value);
                });

                await FileController.UploadFileAsync(server, from_file_path, cts.Token, progress_handler);
                response = await ResponseController.ReceiveResponseAsync(server, cts.Token);
                return response;
     
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                cts.Cancel();
                throw;
            }

        }

        public static async Task DownloadFileAsync(TcpClient server, string to_file_path, Data.Entities.File file, User user, IProgress<double> progress = null)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);
            
            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FilePayload payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(file)), null, null, false, user.Id, file.ParentId.ToString());
                Request request = new(Requests.FILE_DOWNLOAD, Payloads.FILE, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                if (response.ResponseType != Responses.Success)
                {
                    Console.WriteLine($"{response.Payload}");
                }
                else
                {
                    var progress_handler = new Progress<double>(value =>
                    {
                        progress?.Report(value);
                    });

                    await FileController.DownloadFileAsync(server, to_file_path, user.Id, null, cts.Token, progress_handler);
                    response = await ResponseController.ReceiveResponseAsync(server, cts.Token);
                    Console.WriteLine(response.Payload);
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }


        }

        public static async Task<HostingLib.Data.Entities.File> GetFileAsync(TcpClient server, string path, User user)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FilePayload payload = new(null, encryption_controller.EncryptData(path), null, false, user.Id, null);
                Request request = new(Requests.FILE_GET, Payloads.FILE, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                return JsonConvert.DeserializeObject<HostingLib.Data.Entities.File>(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }

        }

        public static async Task<IList<Data.Entities.File>> GetAllFilesAsync(TcpClient server, User user)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FilePayload payload = new(null, null, null, false, user.Id, null);
                Request request = new(Requests.FILE_GETALL, Payloads.FILE, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                return JsonConvert.DeserializeObject<IList<Data.Entities.File>>(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task<IList<Data.Entities.File>> GetPublicFilesAsync(TcpClient server, User user)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FilePayload payload = new(null, null, null, false, user.Id, null);
                Request request = new(Requests.FILE_GET_PUBLIC, Payloads.FILE, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                return JsonConvert.DeserializeObject<IList<Data.Entities.File>>(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task<IList<Data.Entities.File>> GetFilesAsync(TcpClient server, User user, int? parent_id = null)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FilePayload payload = new(null, null, null, false, user.Id, parent_id.ToString());
                Request request = new(Requests.FILE_GET_N, Payloads.FILE, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                return JsonConvert.DeserializeObject<IList<Data.Entities.File>>(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }

        }

        public static async Task MoveFileAsync(TcpClient server, HostingLib.Data.Entities.File file, HostingLib.Data.Entities.File folder)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FilePayload payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(file)),
                    encryption_controller.EncryptData(folder.Path), null, false, 0, null);
                Request request = new(Requests.FILE_MOVE, Payloads.FILE, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task UpdateFilePublicity(TcpClient server, Data.Entities.File file, bool publicity)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FilePayload payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(file)),
                   null, null, publicity, 0, null);
                Request request = new(Requests.FILE_UPDATE_PUBLICITY, Payloads.FILE, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task DeleteFileAsync(TcpClient server, HostingLib.Data.Entities.File file)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FilePayload payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(file)), null, null, false, 0, null);
                Request request = new(Requests.FILE_DELETE, Payloads.FILE, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }

        }

        public static async Task EraseFileAsync(TcpClient server, HostingLib.Data.Entities.File file)
        { 
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FilePayload payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(file)), null, null, false, 0, null);
                Request request = new(Requests.FILE_ERASE, Payloads.FILE, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }


        #endregion

        #region Folder 

        public static async Task CreateFolderAsync(TcpClient server, string folder_name, Data.Entities.User user, Data.Entities.File? parent_folder, bool isPublic)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {

                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FolderPayload payload = new(null, encryption_controller.EncryptData(folder_name), null, isPublic, user.Id, parent_folder?.Id.ToString());
                Request request = new(Requests.FOLDER_CREATE, Payloads.FOLDER, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task MoveFolderAsync(TcpClient server, Data.Entities.File folder, Data.Entities.File folder_to)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FolderPayload payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(folder)), null, 
                    encryption_controller.EncryptData(folder_to.Path), false, 0, null);
                Request request = new(Requests.FOLDER_MOVE, Payloads.FOLDER, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }

        }

        public static async Task UpdateFolderPublicity(TcpClient server, Data.Entities.File folder, bool publicity)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FolderPayload payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(folder)),
                   null, null, publicity, 0, null);
                Request request = new(Requests.FOLDER_UPDATE_PUBLICITY, Payloads.FOLDER, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task DeleteFolderAsync(TcpClient server, Data.Entities.File folder)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FolderPayload payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(folder)), null, null, false, 0, null);
                Request request = new(Requests.FOLDER_DELETE, Payloads.FOLDER, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task EraseFolderAsync(TcpClient server, Data.Entities.File folder)
        {
            CancellationTokenSource cts = new();
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            try
            {
                ClientEncryptionHelper encryption_helper = new(server, key, iv, cts.Token);

                FolderPayload payload = new(encryption_controller.EncryptData(JsonConvert.SerializeObject(folder)), null, null, false, 0, null);
                Request request = new(Requests.FOLDER_ERASE, Payloads.FOLDER, JsonConvert.SerializeObject(payload));

                Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request, cts.Token);

                Console.WriteLine(response.Payload);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
        }

        #endregion
    }
}
