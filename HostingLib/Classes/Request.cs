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
        public string? FileName { get; set; }
        public string? FileDetails { get; set; }
        public int UserId { get; set; }
        public int ParentId {  get; set; }

        public FilePayload(string file, string fileName, string fileDetails, int userId, int parentId)
        {
            File = file;
            FileName = fileName;
            FileDetails = fileDetails;
            UserId = userId;
            ParentId = parentId;
        }
    }

    public class FolderPayload
    {
        public Payloads Type = Payloads.FOLDER;
        public string? Folder { get; set; }
        public string? FolderName { get; set; }
        public string? FolderPath { get; set; }
        public int UserId { get; set; }
        public int ParentId { get; set; }

        public FolderPayload(string folder, string folderName, string folderPath, int userId, int parentId)
        {
            Folder = folder;
            FolderName = folderName;
            FolderPath = folderPath;
            UserId = userId;
            ParentId = parentId;
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
        CLOSE_CONNECTION,

        GET_PUBLIC_KEY,
        ENCRYPTED_DATA,

        USER_SPACE,
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
        FILE_GET_N,
        FILE_DELETE,
        FILE_ERASE,
        FILE_MOVE,

        FOLDER_CREATE,
        FOLDER_GET,
        FOLDER_MOVE,
        FOLDER_DELETE,
        FOLDER_ERASE,
    }

    public enum Payloads
    {
        DATA,
        USER,
        FILE,
        FOLDER,
        MESSAGE,
        PUBLIC_KEY
    }
}
