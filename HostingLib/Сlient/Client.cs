using HostingLib.Classes;
using HostingLib.Controllers;
using HostingLib.Data.Entities;
using HostingLib.Helpers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Сlient
{
    public class Client
    {
        public static async Task<User> GetUserAsync(TcpClient server, string email, string? password)
        {
            var (key, iv) = EncryptionController.GenerateKeyAndIv();
            EncryptionController encryption_controller = new(key, iv);

            string encrypted_email = encryption_controller.EncryptData(email);

            ClientEncryptionHelper encryption_helper = new(server, key, iv);

            UserPayload appended_request_payload = new(null, encrypted_email, null);
            Request appended_request = new(Requests.USER_GET, Payloads.USER, JsonConvert.SerializeObject(appended_request_payload));

            Response response = await encryption_helper.ExchangeEncryptedDataAsync(server, appended_request);

            User user = JsonConvert.DeserializeObject<User>(response.Payload);

            return password is null ? user : await AuthorizationController.Authenticate(user, password);
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
    }
}
