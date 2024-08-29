using HostingLib.Classes;
using HostingLib.Data.Context;
using HostingLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            try
            {
                token.ThrowIfCancellationRequested();
                long file_length = uploaded_file.Length;

                LoggingController.LogDebug($"FileController.UploadFileAsync - Started sending file");
                await TCP.SendFile(client, uploaded_file, file_length, token);
                LoggingController.LogDebug($"FileController.UploadFileAsync - File sent successfully");
            }
            catch (OperationCanceledException)
            {
                LoggingController.LogError("FileController.UploadFileAsync - Operation canceled");
                await uploaded_file.DisposeAsync();
                System.IO.File.Delete(file_path);
                throw;
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is IOException)
            {
                LoggingController.LogError($"FileController.UploadFileAsync - {ex.Message}, deleted file");
                await uploaded_file.DisposeAsync();
                System.IO.File.Delete(file_path);
                throw;
            }
        }

        public static async Task DownloadFileAsync(TcpClient client, string? file_path, CancellationToken token)
        {
            using FileStream new_file = System.IO.File.Create(file_path);
            try
            {
                token.ThrowIfCancellationRequested();

                LoggingController.LogDebug($"FileController.DownloadFileAsync - Started receiving file");

                await TCP.ReceiveFile(client, new_file, token);
                LoggingController.LogDebug($"FileController.DownloadFileAsync - File received successfully");
            }
            catch (OperationCanceledException)
            {
                LoggingController.LogError("FileController.DownloadFileAsync - Operation canceled, deleted file");
                await new_file.DisposeAsync();
                System.IO.File.Delete(file_path);
                throw;
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is IOException)
            {
                LoggingController.LogError($"FileController.DownloadFileAsync - {ex.Message}, deleted file");
                await new_file.DisposeAsync();
                System.IO.File.Delete(file_path);
                throw;
            }
        }

        public static async Task CreateFile(FileDetails info, int user_id, int? parent_id, bool isPublic, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();
                string file_path = Path.Combine(storage_path, user_id.ToString(), info.Name);
                HostingLib.Data.Entities.File file = new(info.Name, file_path, info.Length, info.LastWriteTime, user_id, parent_id, false, false, isPublic);

                context.Files.Add(file);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.CreateFile - file {file.Name} created successfully with path {file.Path} belonging to user {user_id}");
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task<HostingLib.Data.Entities.File> GetFile(string file_path, int user_id, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();

                HostingLib.Data.Entities.File file = await context.Files
                    .Where(f => f.Path == file_path && f.UserId == user_id)
                    .SingleOrDefaultAsync(token);

                if (file is null)
                {
                    LoggingController.LogWarning($"FileController.GetFile - no file found with path {file_path} for user {user_id}");
                }
                else
                {
                    LoggingController.LogInfo($"FileController.GetFile - file {file.Name} found with path {file.Path} for user {user_id}");
                }

                return file;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task<IList<HostingLib.Data.Entities.File>> GetAllFiles(int user_id, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();
                LoggingController.LogDebug($"FileController.GetAllFiles - Fetching all files for user {user_id}");
                IList<HostingLib.Data.Entities.File> files = await context.Files
                    .Where(f => f.UserId == user_id)
                    .ToListAsync(token);

                LoggingController.LogInfo($"FileController.GetAllFiles - Found {files.Count} files for user {user_id}");
                return files;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task<IList<HostingLib.Data.Entities.File>> GetPublicFiles(int user_id, int? parent_id, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();
                LoggingController.LogDebug($"FileController.GetPublicFiles - Fetching all public files for user {user_id}");
                IList<HostingLib.Data.Entities.File> files = await context.Files
                    .Where(f => f.UserId == user_id && f.ParentId == parent_id && f.IsPublic && !f.IsDirectory)
                    .ToListAsync(token);

                LoggingController.LogInfo($"FileController.GetPublicFiles - Found {files.Count} files for user {user_id}");
                return files;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task<IList<HostingLib.Data.Entities.File>> GetFiles(int user_id, int? parent_id, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();
                LoggingController.LogDebug($"FileController.GetFiles - Fetching files for user {user_id} in parent folder {parent_id}");
                IList<HostingLib.Data.Entities.File> files = await context.Files
                    .Where(f => f.UserId == user_id && f.ParentId == parent_id && !f.IsDeleted)
                    .ToListAsync(token);

                LoggingController.LogInfo($"FileController.GetFiles - Found {files.Count} files for user {user_id} in parent folder {parent_id}");
                return files;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task MoveFile(HostingLib.Data.Entities.File file, string folder_to, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();
                string new_path = Path.Combine(folder_to, file.Name);

                LoggingController.LogDebug($"FileController.MoveFile - Moving file {file.Name} from {file.Path} to {new_path}");
                System.IO.File.Move(file.Path, new_path);
                file.Path = new_path;

                context.Files.Update(file);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.MoveFile - File {file.Id} {file.Name} moved successfully to {new_path}");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task UpdateFilePublicity(HostingLib.Data.Entities.File file, bool new_publicity, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();

                file.IsPublic = new_publicity;
                
                context.Files.Update(file);
                await context.SaveChangesAsync(token);

                Console.WriteLine($"File {file.Id} {file.Name} publicity updated");
                LoggingController.LogInfo($"FileController.UpdatePublicity - File {file.Id} {file.Name} publicity updated");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task DeleteFile(HostingLib.Data.Entities.File file, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();
                string new_path = Path.Combine(storage_path, "Deleted", file.Name);

                LoggingController.LogDebug($"FileController.DeleteFile - Moving file {file.Name} to {new_path}");
                System.IO.File.Move(file.Path, new_path);
                file.Path = new_path;
                file.IsDeleted = true;

                context.Files.Update(file);
                await context.SaveChangesAsync(token);

                await CachedDataController.ScheduleFileDeletionAsync(file.Id.ToString(), TimeSpan.FromSeconds(30));

                LoggingController.LogInfo($"FileController.DeleteFile - File {file.Id} {file.Name} marked as deleted and moved to {new_path}");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task EraseFile(HostingLib.Data.Entities.File file, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();

                LoggingController.LogDebug($"FileController.EraseFile - Erasing file {file.Name} from {file.Path}");
                context.Files.Remove(file);
                await context.SaveChangesAsync(token);

                System.IO.File.Delete(file.Path);
                LoggingController.LogInfo($"FileController.EraseFile - File {file.Id} {file.Name} erased successfully");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task EraseFile(int file_id, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();

                Data.Entities.File file = await context.Files
                    .Where(f => f.Id == file_id)
                    .SingleOrDefaultAsync(token);

                LoggingController.LogDebug($"FileController.EraseFile - Erasing file {file.Name} from {file.Path}");
                context.Files.Remove(file);
                await context.SaveChangesAsync(token);

                System.IO.File.Delete(file.Path);
                LoggingController.LogInfo($"FileController.EraseFile - File {file.Id} {file.Name} erased successfully");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        #endregion

        #region Folder

        public static async Task CreateFolder(string folder_name, int user_id, int? parent_id, bool isPublic, CancellationToken token)
        {
            using HostingDbContext context = new();
            string parent_path = Path.Combine(storage_path, user_id.ToString());
            string folder_path = Path.Combine(parent_path, folder_name);

            try
            {
                token.ThrowIfCancellationRequested();

                if (Directory.Exists(folder_path))
                {
                    LoggingController.LogError($"FileController.CreateFolder - error: folder {folder_name} already exists at {folder_path}");
                    throw new ArgumentException("Such folder already exists!");
                }

                Data.Entities.File folder = new(folder_name, folder_path, 0, DateTime.Now, user_id, parent_id, false, true, isPublic);
                context.Files.Add(folder);
                Directory.CreateDirectory(folder_path);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.CreateFolder - folder {folder_name} created successfully at {folder_path} for user {user_id}");
            }
            catch (OperationCanceledException)
            {
                LoggingController.LogError($"FileController.CreateFolder - operation canceled, deleting folder {folder_path}");
                Directory.Delete(folder_path);
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task<Data.Entities.File> GetFolder(string folder_name, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();
                LoggingController.LogDebug($"FileController.GetFolder - fetching folder {folder_name}");
                Data.Entities.File folder = await context.Files
                    .Where(f => f.IsDirectory && f.Name == folder_name)
                    .SingleOrDefaultAsync(token);

                if (folder is null)
                {
                    LoggingController.LogWarning($"FileController.GetFolder - folder {folder_name} not found");
                }
                else
                {
                    LoggingController.LogInfo($"FileController.GetFolder - folder {folder.Name} found");
                }

                return folder;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task MoveFolder(Data.Entities.File folder, string folder_to, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();

                context.Files.Include(f => f.Children);
                string new_folder_path = Path.Combine(folder_to, folder.Name);

                LoggingController.LogDebug($"FileController.MoveFolder - moving folder {folder.Name} to {new_folder_path}");
                Directory.Move(folder.Path, new_folder_path);
                folder.Path = new_folder_path;

                foreach (Data.Entities.File file in folder.Children)
                {
                    string new_path = Path.Combine(new_folder_path, file.Name);
                    file.Path = new_path;
                    context.Update(file);
                    LoggingController.LogInfo($"FileController.MoveFolder - file {file.Id} {file.Name} moved to {new_path}");
                }

                context.Update(folder);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.MoveFolder - folder {folder.Name} moved successfully to {new_folder_path}");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task UpdateFolderPublicity(Data.Entities.File folder, bool publicity, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();

                context.Files.Include(f => f.Children);

                folder.IsPublic = publicity;

                await UpdateChildPublicityAsync(folder.Children, publicity, context, token);

                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.UpdateFolderPublicity - folder {folder.Name} publicity updated");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }
        private static async Task UpdateChildPublicityAsync(
    ICollection<Data.Entities.File> children, bool publicity, HostingDbContext context, CancellationToken token)
        {
            foreach (Data.Entities.File child in children)
            {
                token.ThrowIfCancellationRequested();

                child.IsPublic = publicity;
                context.Files.Update(child);

                if (child.IsDirectory)
                {
                    await context.Entry(child).Collection(f => f.Children).LoadAsync(token);
                    await UpdateChildPublicityAsync(child.Children, publicity, context, token);
                }
            }
        }

        public static async Task DeleteFolder(Data.Entities.File folder, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();

                context.Files.Include(f => f.Children);
                string new_folder_path = Path.Combine(storage_path, "Deleted", folder.Name);

                LoggingController.LogDebug($"FileController.DeleteFolder - moving folder {folder.Name} to {new_folder_path}");
                Directory.Move(folder.Path, new_folder_path);
                folder.Path = new_folder_path;

                foreach (Data.Entities.File file in folder.Children)
                {
                    string new_path = Path.Combine(new_folder_path, file.Name);
                    file.Path = new_path;
                    context.Update(file);
                    LoggingController.LogInfo($"FileController.DeleteFolder - file {file.Id} {file.Name} marked as deleted and moved to {new_path}");
                }

                context.Update(folder);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.DeleteFolder - folder {folder.Name} moved to deleted successfully");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task EraseFolder(Data.Entities.File folder, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();

                context.Files.Include(f => f.Children);
                LoggingController.LogDebug($"FileController.EraseFolder - erasing folder {folder.Name} at {folder.Path}");

                Directory.Delete(folder.Path, true);

                foreach (Data.Entities.File file in folder.Children)
                {
                    context.Files.Remove(file);
                    LoggingController.LogInfo($"FileController.EraseFolder - file {file.Id} {file.Name} erased");
                }

                context.Files.Remove(folder);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.EraseFolder - folder {folder.Name} and its files erased successfully");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task EraseFolder(int file_id, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();

                context.Files.Include(f => f.Children);

                Data.Entities.File folder = await context.Files
                    .Where(f => f.Id == file_id)
                    .SingleOrDefaultAsync(token);

                LoggingController.LogDebug($"FileController.EraseFolder - erasing folder {folder.Name} at {folder.Path}");

                Directory.Delete(folder.Path, true);

                foreach (Data.Entities.File file in folder.Children)
                {
                    context.Files.Remove(file);
                    LoggingController.LogInfo($"FileController.EraseFolder - file {file.Id} {file.Name} erased");
                }

                context.Files.Remove(folder);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.EraseFolder - folder {folder.Name} and its files erased successfully");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        #endregion
    }
}
