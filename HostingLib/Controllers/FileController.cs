using HostingLib.Classes;
using HostingLib.Data.Context;
using HostingLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPLib;

namespace HostingLib.Controllers
{
    public class FileController
    {
        public readonly static string storage_path = @"../../../Files";

        public static async Task UploadFileAsync(TcpClient client, string? file_path)
        {
            using FileStream uploaded_file = new(file_path, FileMode.Open, FileAccess.Read);
            long file_length = uploaded_file.Length;
            await TCP.SendFile(client, uploaded_file, file_length);
        }

        public static async Task DownloadFileAsync(TcpClient client, string? file_path)
        {
            using FileStream new_file = System.IO.File.Create(file_path);
            await TCP.ReceiveFile(client, new_file);
        }

        public static async Task CreateFile(FileDetails info, int user_id)
        {
            HostingDbContext context = new();

            string file_path = Path.Combine(storage_path, user_id.ToString(), info.Name);
            HostingLib.Data.Entities.File file = new(info.Name, file_path, user_id);

            context.Files
                .Add(file);

            await context.SaveChangesAsync();
            await context.DisposeAsync();

            Console.WriteLine($"File {file.Name} created successfully with path {file.Path} belonging to {user_id}");
        }

        public static async Task<IList<HostingLib.Data.Entities.File>> GetAllFiles(int user_id)
        {
            HostingDbContext context = new();

            IList<HostingLib.Data.Entities.File> files = await context.Files
                .Where(f => f.UserId == user_id)
                .ToListAsync();

            await context.DisposeAsync();

            foreach (Data.Entities.File file in files)
            {
                Console.WriteLine($"{file.Id} {file.Name}");
            }

            return files;
        }

        public static async Task<HostingLib.Data.Entities.File> GetFile(string file_name, int user_id)
        {
            HostingDbContext context = new();

            HostingLib.Data.Entities.File file = await context.Files
                .Where(f => f.Name == file_name && f.UserId == user_id)
                .SingleOrDefaultAsync();

            await context.DisposeAsync();

            if(file is null)
            {
                Console.WriteLine($"Get request with data: {file_name} {user_id}, returned null - no such file belonging to user");
            }
            else
            {
                Console.WriteLine($"Get request with data: {file_name} {user_id}, returned file {file.Name} {file.Path}");
            }

            return file;
        }

        public static async Task DeleteFile(HostingLib.Data.Entities.File file)
        {
            HostingDbContext context = new();

            context.Files
                .Remove(file);

            await context.SaveChangesAsync();
            await context.DisposeAsync();

            System.IO.File.Delete(file.Path);

            Console.WriteLine($"File {file.Id} {file.Name} deleted successfully!");
        }
    }
}
