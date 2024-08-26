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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPLib;

namespace HostingLib.Controllers
{
    public class FileController
    {
        public readonly static string storage_path = @"../../../Files";

        #region File

        public static async Task UploadFileAsync(TcpClient client, string? file_path, CancellationToken token)
        {
            using FileStream uploaded_file = new(file_path, FileMode.Open, FileAccess.Read);
            long file_length = uploaded_file.Length;
            await TCP.SendFile(client, uploaded_file, file_length, token);
        }

        public static async Task DownloadFileAsync(TcpClient client, string? file_path, CancellationToken token)
        {
            using FileStream new_file = System.IO.File.Create(file_path);
            await TCP.ReceiveFile(client, new_file, token);
        }

        public static async Task CreateFile(FileDetails info, int user_id, int parent_id, CancellationToken token)
        {
            HostingDbContext context = new();

            string file_path = Path.Combine(storage_path, user_id.ToString(), info.Name);
            HostingLib.Data.Entities.File file = new(info.Name, file_path, info.Length, info.LastWriteTime, user_id, parent_id);

            if (System.IO.File.Exists(file_path)) throw new ArgumentException("Such file already exists!");

            context.Files
                .Add(file);

            await context.SaveChangesAsync(token);
            await context.DisposeAsync();

            Console.WriteLine($"File {file.Name} created successfully with path {file.Path} belonging to {user_id}");
        }

        public static async Task<HostingLib.Data.Entities.File> GetFile(string file_path, int user_id, CancellationToken token)
        {
            HostingDbContext context = new();

            HostingLib.Data.Entities.File file = await context.Files
                .Where(f => f.Path == file_path && f.UserId == user_id)
                .SingleOrDefaultAsync(token);

            await context.DisposeAsync();

            if(file is null)
            {
                Console.WriteLine($"Get request with data: {file_path} {user_id}, returned null - no such file belonging to user");
            }
            else
            {
                Console.WriteLine($"Get request with data: {file_path} {user_id}, returned file {file.Name} {file.Path}");
            }

            return file;
        }

        public static async Task<IList<HostingLib.Data.Entities.File>> GetAllFiles(int user_id, CancellationToken token)
        {
            HostingDbContext context = new();

            IList<HostingLib.Data.Entities.File> files = await context.Files
                .Where(f => f.UserId == user_id)
                .ToListAsync(token);

            await context.DisposeAsync();

            foreach (Data.Entities.File file in files)
            {
                Console.WriteLine($"{file.Id} {file.Name}");
            }

            return files;
        }

        public static async Task<IList<HostingLib.Data.Entities.File>> GetFiles(int user_id, int parent_id, CancellationToken token)
        {
            HostingDbContext context = new();

            IList<HostingLib.Data.Entities.File> files = await context.Files
                .Where(f => f.UserId == user_id && f.ParentId == parent_id)
                .ToListAsync(token);

            await context.DisposeAsync();

            foreach (Data.Entities.File file in files)
            {
                Console.WriteLine($"{file.Id} {file.Name}");
            }

            return files;
        }

        public static async Task MoveFile(HostingLib.Data.Entities.File file, string folder_to, CancellationToken token)
        {
            HostingDbContext context = new();

            string new_path = Path.Combine(folder_to, file.Name);
            file.Path = new_path;
            System.IO.File.Move(file.Path, new_path);

            context.Files.
                Update(file);

            await context.SaveChangesAsync(token);
            await context.DisposeAsync();

            Console.WriteLine($"File {file.Id} {file.Name} moved to {new_path}");
        }

        public static async Task DeleteFile(HostingLib.Data.Entities.File file, CancellationToken token)
        {
            HostingDbContext context = new();

            string new_path = Path.Combine(storage_path, "Deleted", file.Name);
            Directory.Move(file.Path, new_path);
            file.Path = new_path;
            file.IsDeleted = true;
            file.ParentId = -2;

            context.Files
                .Update(file);

            await context.SaveChangesAsync(token);
            await context.DisposeAsync();

            Console.WriteLine($"File {file.Id} {file.Name} moved to deleted successfully!");
        }

        public static async Task EraseFile(HostingLib.Data.Entities.File file, CancellationToken token)
        {
            HostingDbContext context = new();

            context.Files
                .Remove(file);

            await context.SaveChangesAsync(token);
            await context.DisposeAsync();

            System.IO.File.Delete(file.Path);

            Console.WriteLine($"File {file.Id} {file.Name} erased successfully!");
        }

        #endregion

        #region Folder

        public static async Task CreateFolder(string folder_name, int user_id, CancellationToken token, int parent_id = -1)
        {
            HostingDbContext context = new();

            string parent_path = Path.Combine(storage_path, user_id.ToString());

            string folder_path = Path.Combine(parent_path, folder_name);

            Data.Entities.File folder = new(folder_name, folder_path, 0, DateTime.Now, user_id, parent_id, false, true);

            if (Directory.Exists(folder_path)) throw new ArgumentException("Such folder already exists!");

            context.Files
                .Add(folder);

            Directory.CreateDirectory(folder_path);

            await context.SaveChangesAsync(token);
            await context.DisposeAsync();


            Console.WriteLine($"Added folder successfully with name {folder_name}, belonging to {user_id}");
        }

        public static async Task<Data.Entities.File> GetFolder(string folder_name, CancellationToken token)
        {
            HostingDbContext context = new();

            return await context.Files
                .Where(f => f.IsDirectory && f.Name == folder_name)
                .SingleOrDefaultAsync(token);
                
        }

        public static async Task MoveFolder(Data.Entities.File folder, string folder_to, CancellationToken token)
        {
            HostingDbContext context = new();

            context.Files
                .Include(f => f.Parent)
                .Include(f => f.Children);

            string new_folder_path = Path.Combine(folder_to, folder.Name);
            Directory.Move(folder.Path, new_folder_path);
            folder.Path = new(folder.Path);

            foreach(Data.Entities.File file in folder.Children)
            {
                string new_path = Path.Combine(new_folder_path, file.Name);
                file.Path = new_path;
                context.Update(file);
                Console.WriteLine($"File {file.Id} {file.Name} moved to {new_path}");
            }

            await context.SaveChangesAsync(token);
            await context.DisposeAsync();

            Console.WriteLine($"Folder {folder.Name} moved successfully to {new_folder_path}");
        }

        public static async Task DeleteFolder(Data.Entities.File folder, CancellationToken token)
        {
            HostingDbContext context = new();

            context.Files
                .Include(f => f.Parent)
                .Include(f => f.Children);

            string new_folder_path = Path.Combine(storage_path, "Deleted", folder.Name);
            Directory.Move(folder.Path, new_folder_path);
            folder.Path = new_folder_path;

            foreach(Data.Entities.File file in folder.Children)
            {
                string new_path = file.Parent == null ? 
                    Path.Combine(storage_path, "Deleted", file.Name) 
                    : Path.Combine(storage_path, file.Parent.Name, "Deleted", file.Name);
                file.Path = new_path;
                context.Files
                    .Update(file);

                Console.WriteLine($"File {file.Id} {file.Name} moved to deleted");
            }


            context.
                Update(folder);

            await context.SaveChangesAsync(token);
            await context.DisposeAsync();

            Console.WriteLine($"Folder {folder.Name} moved to deleted successfully!");
        }

        public static async Task EraseFolder(Data.Entities.File folder, CancellationToken token)
        {
            HostingDbContext context = new();

            context.Files
                .Include(f => f.Children);

            Directory.Delete(folder.Path, true);

            foreach(Data.Entities.File file in folder.Children)
            {
                context.Files.Remove(file);
            }

            context.Files.Remove(folder);

            await context.SaveChangesAsync(token);
            await context.DisposeAsync();

            Console.WriteLine("Folder and files erased successfully!");
        }

        #endregion
    }
}
