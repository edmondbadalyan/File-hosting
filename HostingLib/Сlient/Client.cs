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
using System.Threading.Tasks;

namespace HostingLib.Сlient
{
    public class Client
    {
        #region User

        public static async Task<User> GetUserAsync(TcpClient server, string email) {
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            string encrypted_email = encryption_controller.EncryptData(email);

            ClientEncryptionHelper encryption_helper = new(server, key, iv);

            UserPayload appended_request_payload = new(null, encrypted_email, null);
            Request appended_request = new(Requests.USER_GET, Payloads.USER, JsonConvert.SerializeObject(appended_request_payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, appended_request);

            return JsonConvert.DeserializeObject<User>(response.Payload);
        }

        public static async Task<User> AuthenticateUserAsync(TcpClient server, User user, string given_password)
        {
            EncryptionController encryption_controller = new(user.EncryptionKey, user.Iv);

            string encrypted_password = encryption_controller.EncryptData(given_password);

            ClientEncryptionHelper encryption_helper = new(server, user.EncryptionKey, user.Iv);

            UserPayload request_payload = new(JsonConvert.SerializeObject(user), null, encrypted_password);
            Request request = new(Requests.USER_AUTHENTICATE, Payloads.USER, JsonConvert.SerializeObject(request_payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request);

            return JsonConvert.DeserializeObject<User>(response.Payload);
        }

        public static async Task CreateUserAsync(TcpClient server, string email, string password)
        {
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            string encrypted_email = encryption_controller.EncryptData(email);
            string encrypted_password = encryption_controller.EncryptData(password);

            ClientEncryptionHelper encryption_helper = new(server, key, iv);

            UserPayload appended_request_payload = new(null, encrypted_email, encrypted_password);
            Request appended_request = new(Requests.USER_CREATE, Payloads.USER, JsonConvert.SerializeObject(appended_request_payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, appended_request);
            Console.WriteLine(response.Payload);
        }

        public static async Task UpdateUserAsync(TcpClient server, User user, string password)
        {
            EncryptionController encryption_controller = new(user.EncryptionKey, user.Iv);
            string encrypted_password = encryption_controller.EncryptData(password);

            ClientEncryptionHelper encryption_helper = new(server, user.EncryptionKey, user.Iv);

            UserPayload request_payload = new(JsonConvert.SerializeObject(user), null, encrypted_password);
            Request request = new(Requests.USER_UPDATE, Payloads.USER, JsonConvert.SerializeObject(request_payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request);
            Console.WriteLine(response.Payload);
        }

        public static async Task DeleteUserAsync(TcpClient server, User user)
        {
            ClientEncryptionHelper encryption_helper = new(server, user.EncryptionKey, user.Iv);

            UserPayload request_payload = new(JsonConvert.SerializeObject(user), null, null);
            Request request = new(Requests.USER_DELETE, Payloads.USER, JsonConvert.SerializeObject(request_payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request);
            Console.WriteLine(response.Payload);
        }
        #endregion

        #region File

        public static async Task UploadFileAsync(TcpClient server, string from_file_path, User user, Data.Entities.File parent)
        {
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            ClientEncryptionHelper encryption_helper = new(server, key, iv);

            FileInfo info = new(from_file_path);
            FileDetails details = new(info.Name, info.Length, info.Extension, info.CreationTime);
            FilePayload payload = new(null, JsonConvert.SerializeObject(details), user.Id, parent.Id);
            Request request = new(Requests.FILE_UPLOAD, Payloads.FILE, JsonConvert.SerializeObject(payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request);

            if(response.ResponseType == Responses.Success)
            {
                await FileController.UploadFileAsync(server, from_file_path);
            }

            response = await ResponseController.ReceiveResponseAsync(server);
            Console.WriteLine(response.Payload);
        }

        public static async Task DownloadFileAsync(TcpClient server, string to_file_path, Data.Entities.File file, User user)
        {
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            ClientEncryptionHelper encryption_helper = new(server, key, iv);

            FilePayload payload = new(JsonConvert.SerializeObject(file), null, user.Id, 0);
            Request request = new(Requests.FILE_DOWNLOAD, Payloads.FILE, JsonConvert.SerializeObject(payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request);

            if (response.ResponseType == Responses.Success)
            {
                await FileController.DownloadFileAsync(server, to_file_path);
            }

            response = await ResponseController.ReceiveResponseAsync(server);
            Console.WriteLine(response.Payload);

        }

        public static async Task<IList<Data.Entities.File>> GetAllFilesAsync(TcpClient server, User user, int parent_id = -1)
        {
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            ClientEncryptionHelper encryption_helper = new(server, key, iv);

            FilePayload payload = new(null, null, user.Id, parent_id);
            Request request = new(Requests.FILE_GETALL, Payloads.FILE, JsonConvert.SerializeObject(payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request);

            return JsonConvert.DeserializeObject<IList<Data.Entities.File>>(response.Payload);
        }

        public static async Task<HostingLib.Data.Entities.File> GetFileAsync(TcpClient server, HostingLib.Data.Entities.File file, User user)
        {
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            ClientEncryptionHelper encryption_helper = new(server, key, iv);

            FilePayload payload = new(JsonConvert.SerializeObject(file), null, user.Id, 0);
            Request request = new(Requests.FILE_GET, Payloads.FILE, JsonConvert.SerializeObject(payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request);

            return JsonConvert.DeserializeObject<HostingLib.Data.Entities.File>(response.Payload);
        }

        public static async Task DeleteFileAsync(TcpClient server, HostingLib.Data.Entities.File file)
        {
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            ClientEncryptionHelper encryption_helper = new(server, key, iv);

            FilePayload payload = new(JsonConvert.SerializeObject(file), null, 0, 0);
            Request request = new(Requests.FILE_DELETE, Payloads.FILE, JsonConvert.SerializeObject(payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request);

            Console.WriteLine(response.Payload);
        }

        #endregion

        #region Folder 

        public static async Task CreateFolderAsync(TcpClient server, string folder_name, int user_id, int parent_id = -1)
        {
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            ClientEncryptionHelper encryption_helper = new(server, key, iv);

            FolderPayload payload = new(null, folder_name, null, user_id, parent_id);
            Request request = new(Requests.FOLDER_CREATE, Payloads.FOLDER, JsonConvert.SerializeObject(payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request);

            Console.WriteLine(response.Payload);
        }

        public static async Task MoveFolderAsync(TcpClient server, Data.Entities.File folder, Data.Entities.File folder_to)
        {
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            ClientEncryptionHelper encryption_helper = new(server, key, iv);

            FolderPayload payload = new(JsonConvert.SerializeObject(folder), null, folder_to.Path, 0, 0);
            Request request = new(Requests.FOLDER_MOVE, Payloads.FOLDER, JsonConvert.SerializeObject(payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request);

            Console.WriteLine(response.Payload);
        }

        public static async Task DeleteFolderAsync(TcpClient server, Data.Entities.File folder)
        {
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            ClientEncryptionHelper encryption_helper = new(server, key, iv);

            FolderPayload payload = new(JsonConvert.SerializeObject(folder), null, null, 0, 0);
            Request request = new(Requests.FOLDER_DELETE, Payloads.FOLDER, JsonConvert.SerializeObject(payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request);

            Console.WriteLine(response.Payload);
        }

        public static async Task EraseFolderAsync(TcpClient server, Data.Entities.File folder)
        {
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            ClientEncryptionHelper encryption_helper = new(server, key, iv);

            FolderPayload payload = new(JsonConvert.SerializeObject(folder), null, null, 0, 0);
            Request request = new(Requests.FOLDER_ERASE, Payloads.FOLDER, JsonConvert.SerializeObject(payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, request);

            Console.WriteLine(response.Payload);
        }

        #endregion
    }
}
