using HostingLib.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Classes
{
    public class UserPayload
    {
        public Payloads Type = Payloads.USER;
        public string? User { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }

        public UserPayload(string user, string email, string password)
        {
            User = user;
            Email = email;
            Password = password;
        }
    }

    public class FilePayload
    {
        public Payloads Type = Payloads.FILE;
        public string? File { get; set; }
        public string? FileDetails { get; set; }
        public int UserId { get; set; }

        public FilePayload(string file, string fileInfo, int userId)
        {
            File = file;
            FileDetails = fileInfo;
            UserId = userId;
        }
    }

    public class EncryptedDataPayload
    {
        public Payloads Type = Payloads.DATA;
        public byte[] Key { get; set; }
        public byte[] Iv { get; set; }
        public string AppendedRequest { get; set; }

        public EncryptedDataPayload(byte[] key, byte[] iv, string appendedRequest)
        {
            Key = key;
            Iv = iv;
            AppendedRequest = appendedRequest;
        }
    }

    public class Request
    {
        public Requests RequestType { get; set; }
        public Payloads PayloadType { get; set; }
        public string Payload { get; set; } // Данные запроса, можно использовать другой тип или сериализовать JSON

        public Request(Requests requestType, Payloads payloadType, string payload)
        {
            RequestType = requestType;
            PayloadType = payloadType;
            Payload = payload;
        }
    }

    public enum Requests
    {
        GET_PUBLIC_KEY,
        ENCRYPTED_DATA,
        USER_CREATE,
        USER_GET,
        USER_AUTHENTICATE,
        USER_UPDATE,
        USER_DELETE,
        FILE_UPLOAD,
        FILE_DOWNLOAD,
        FILE_CREATE,
        FILE_GET,
        FILE_GETALL,
        FILE_DELETE,
        FILE_MOVE,
    }

    public enum Payloads
    {
        DATA,
        USER,
        FILE,
        MESSAGE,
        PUBLIC_KEY
    }
}
