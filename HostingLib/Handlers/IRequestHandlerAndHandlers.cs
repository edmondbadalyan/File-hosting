using HostingLib.Classes;
using HostingLib.Controllers;
using HostingLib.Data.Entities;
using HostingLib.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Handlers
{
    public interface IRequestHandler<TResult>
    {
        Task<TResult> HandleAsync(Classes.Request request);
    }

    public class GetPublicKeyHandler : IRequestHandler<Response>
    {
        public Task<Response> HandleAsync(Request request)
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
        public async Task<Response> HandleAsync(Request request)
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

                switch(appended_request.PayloadType)
                {
                    case Payloads.USER:
                        {
                            string user = null;
                            string email = null, password = null;

                            appended_request_payload = JsonConvert.DeserializeObject<UserPayload>(appended_request.Payload);

                            if(appended_request_payload.User != null)
                            {
                                user = appended_request_payload.User;
                            }

                            if(appended_request_payload.Email != null)
                            {
                                email = encryption_controller.DecryptData(appended_request_payload.Email);
                            }

                            if(appended_request_payload.Password != null)
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
                            throw new NotImplementedException();
                        }
                }
                
                Response response = await RequestController.HandleRequestAsync<Response>(decrypted_request);
                return response;
            }
            catch (Exception ex)
            {
                return new(Responses.Fail, Payloads.MESSAGE, ex.Message);
            }
        }
    }

    public class CreateUserHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(Request request)
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
        public async Task<Response> HandleAsync(Request request)
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

    public class AuthorizeUserHandler : IRequestHandler<Response>
    {
        public async Task<Response> HandleAsync(Request request)
        {
            try
            {
                UserPayload payload = JsonConvert.DeserializeObject<UserPayload>(request.Payload);
                User user = await AuthorizationController.Authenticate(JsonConvert.DeserializeObject<User>(payload.User), payload.Password);
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
        public async Task<Response> HandleAsync(Request request)
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

}
