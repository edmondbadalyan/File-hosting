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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPLib;

namespace HostingLib.Controllers
{
    public class FileController
    {
        public const string StoragePath = @"../../../Files";
        public const string CachePrefix = "Files:";

        #region File

        public static async Task UploadFileAsync(TcpClient client, string? file_path, CancellationToken token, IProgress<double> progress = null)
        {
            using FileStream uploaded_file = new(file_path, FileMode.Open, FileAccess.Read);
            try
            {
                token.ThrowIfCancellationRequested();
                long file_length = uploaded_file.Length;

                LoggingController.LogDebug($"FileController.UploadFileAsync - Started sending file");
                await TCP.SendFile(client, uploaded_file, file_length, token, progress);
                LoggingController.LogDebug($"FileController.UploadFileAsync - File sent successfully");
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"FileController.UploadFileAsync - {ex.Message}, deleted file");
                await uploaded_file.DisposeAsync();
                System.IO.File.Delete(file_path);
                throw;
            }
        }

        public static async Task DownloadFileAsync(TcpClient client, string? file_path, int user_id, int? parent_id, CancellationToken token, IProgress<double> progress = null)
        {
            using FileStream new_file = System.IO.File.Create(file_path);
            try
            {
                token.ThrowIfCancellationRequested();

                LoggingController.LogDebug($"FileController.DownloadFileAsync - Started receiving file");

                await TCP.ReceiveFile(client, new_file, token, progress);
                LoggingController.LogDebug($"FileController.DownloadFileAsync - File received successfully");

                await CachedDataController.RemoveCacheAsync($"{UserController.CachePrefix}space:{user_id}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}all:{user_id}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{user_id}:{parent_id}");

                LoggingController.LogDebug($"FileController.DownloadFileAsync - Cleaned up cache");
            }
            catch (Exception ex)
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
                string file_path = Path.Combine(StoragePath, user_id.ToString(), info.Name);
                Data.Entities.File file = new(info.Name, file_path, info.Length, info.LastWriteTime, user_id, parent_id, false, false, isPublic);

                context.Files.Add(file);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.CreateFile - file {file.Name} created successfully with path {file.Path} belonging to user {user_id}");

                await CachedDataController.RemoveCacheAsync($"{CachePrefix}all:{user_id}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{user_id}:{parent_id}");
                LoggingController.LogDebug($"FileController.CreateFile - Cleaned up cache");
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"FileController.CreateFile - {ex.Message}");
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task<Data.Entities.File> GetFile(string file_path, int user_id, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();

                Data.Entities.File file = await context.Files
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

        public static async Task<IList<Data.Entities.File>> GetAllFiles(int user_id, CancellationToken token)
        {
            string cache_key = $"{CachePrefix}all:{user_id}";

            IList<Data.Entities.File> cached_files = await CachedDataController.GetValueAsync<IList<Data.Entities.File>>(cache_key);
            if(cached_files != null)
            {
                LoggingController.LogInfo($"FileController.GetAllFiles - Retrieved {cached_files.Count} files for user {user_id} from cache.");
                return cached_files;
            }

            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();
                LoggingController.LogDebug($"FileController.GetAllFiles - Fetching all files for user {user_id}");
                IList<Data.Entities.File> files = await context.Files
                    .Where(f => f.UserId == user_id)
                    .ToListAsync(token);

                LoggingController.LogInfo($"FileController.GetAllFiles - Found {files.Count} files for user {user_id}");

                await CachedDataController.SetValueAsync(cache_key, files, TimeSpan.FromHours(1));

                return files;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task<IList<Data.Entities.File>> GetPublicFiles(int user_id, CancellationToken token)
        {
            string cache_key = $"{CachePrefix}public:{user_id}";

            IList<Data.Entities.File> cached_files = await CachedDataController.GetValueAsync<IList<Data.Entities.File>>(cache_key);
            if (cached_files != null)
            {
                LoggingController.LogInfo($"FileController.GetPubliclFiles - Retrieved {cached_files.Count} files for user {user_id} from cache.");
                return cached_files;
            }

            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();
                LoggingController.LogDebug($"FileController.GetPublicFiles - Fetching all public files for user {user_id}");
                IList<Data.Entities.File> files = await context.Files
                    .Where(f => f.UserId == user_id && f.IsPublic && !f.IsDirectory)
                    .ToListAsync(token);

                LoggingController.LogInfo($"FileController.GetPublicFiles - Found {files.Count} files for user {user_id}");
                await CachedDataController.SetValueAsync(cache_key, files, TimeSpan.FromHours(1));
                return files;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task<IList<Data.Entities.File>> GetFiles(int user_id, int? parent_id, CancellationToken token)
        {
            string cache_key = $"{CachePrefix}{user_id}:{parent_id}";

            IList<Data.Entities.File> cached_files = await CachedDataController.GetValueAsync<IList<Data.Entities.File>>(cache_key);
            if (cached_files != null)
            {
                LoggingController.LogInfo($"FileController.GetFiles - Retrieved {cached_files.Count} files for user {user_id} from cache.");
                return cached_files;
            }

            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();
                LoggingController.LogDebug($"FileController.GetFiles - Fetching files for user {user_id} in parent folder {parent_id}");
                IList<Data.Entities.File> files = await context.Files
                    .Where(f => f.UserId == user_id && f.ParentId == parent_id && !f.IsDeleted)
                    .ToListAsync(token);

                LoggingController.LogInfo($"FileController.GetFiles - Found {files.Count} files for user {user_id} in parent folder {parent_id}");
                await CachedDataController.SetValueAsync(cache_key, files, TimeSpan.FromHours(1));
                return files;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task MoveFile(Data.Entities.File file, string folder_to, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();

                Data.Entities.File new_folder = await context.Files
                    .SingleOrDefaultAsync(f => f.Path == folder_to, token);

                if(new_folder == null)
                {
                    throw new InvalidOperationException("There is no such folder!");
                }

                string new_path = Path.Combine(folder_to, file.Name);

                LoggingController.LogDebug($"FileController.MoveFile - Moving file {file.Name} from {file.Path} to {new_path}");
                System.IO.File.Move(file.Path, new_path);
                file.Path = new_path;
                file.ParentId = new_folder.Id;

                context.Files.Update(file);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.MoveFile - File {file.Id} {file.Name} moved successfully to {new_path}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{file.UserId}:{file.ParentId}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{file.UserId}:{new_folder.ParentId}");
                LoggingController.LogDebug($"FileController.MoveFile - Cleaned up cache");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task UpdateFilePublicity(Data.Entities.File file, bool new_publicity, CancellationToken token)
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

                await CachedDataController.RemoveCacheAsync($"{CachePrefix}public:{file.UserId}");
                LoggingController.LogDebug($"FileController.UpdateFilePublicity - Cleaned up cache");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task DeleteFile(Data.Entities.File file, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();
                string new_path = Path.Combine(StoragePath, "Deleted", file.Name);

                LoggingController.LogDebug($"FileController.DeleteFile - Moving file {file.Name} to {new_path}");
                System.IO.File.Move(file.Path, new_path);
                file.Path = new_path;
                file.IsDeleted = true;

                context.Files.Update(file);
                await context.SaveChangesAsync(token);

                User user = await UserController.GetUserByIdAsync(file.UserId, token);

                await CachedDataController.ScheduleFileDeletionAsync(file.Id.ToString(), user.AutoFileDeletionTime);

                LoggingController.LogInfo($"FileController.DeleteFile - File {file.Id} {file.Name} marked as deleted and moved to {new_path}");

                await CachedDataController.RemoveCacheAsync($"{CachePrefix}all:{file.UserId}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{file.UserId}:{file.ParentId}");
                LoggingController.LogDebug($"FileController.DeleteFile - Cleaned up cache");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public static async Task EraseFile(Data.Entities.File file, CancellationToken token)
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

                await CachedDataController.RemoveCacheAsync($"{UserController.CachePrefix}space:{file.UserId}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}all:{file.UserId}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{file.UserId}:{file.ParentId}");
                LoggingController.LogDebug($"FileController.EraseFile - Cleaned up cache");
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

                await CachedDataController.RemoveCacheAsync($"{UserController.CachePrefix}space:{file.UserId}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}all:{file.UserId}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{file.UserId}:{file.ParentId}");
                LoggingController.LogDebug($"FileController.EraseFile - Cleaned up cache");
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
            string parent_path = Path.Combine(StoragePath, user_id.ToString());
            string folder_path = Path.Combine(parent_path, folder_name);

            try
            {
                token.ThrowIfCancellationRequested();

                if (Directory.Exists(folder_path))
                {
                    LoggingController.LogError($"FileController.CreateFolder - Error: folder {folder_name} already exists at {folder_path}");
                    throw new ArgumentException("Such folder already exists!");
                }

                Data.Entities.File folder = new(folder_name, folder_path, 0, DateTime.Now, user_id, parent_id, false, true, isPublic);
                context.Files.Add(folder);
                Directory.CreateDirectory(folder_path);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.CreateFolder - folder {folder_name} created successfully at {folder_path} for user {user_id}");

                await CachedDataController.RemoveCacheAsync($"{CachePrefix}all:{user_id}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{user_id}:{parent_id}");
                LoggingController.LogDebug($"FileController.CreateFolder - Cleaned up cache");
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

                await LoadFolderChildrenRecursive(folder, context, token);
                string new_folder_path = Path.Combine(folder_to, folder.Name);

                Data.Entities.File new_folder = await context.Files
                    .SingleOrDefaultAsync(f => f.Path == folder_to, token);

                if (new_folder == null)
                {
                    throw new InvalidOperationException("There is no such folder!");
                }

                LoggingController.LogDebug($"FileController.MoveFolder - moving folder {folder.Name} to {new_folder_path}");
                Directory.Move(folder.Path, new_folder_path);
                folder.Path = new_folder_path;

                await UpdateFolderPathsRecursive(folder, new_folder_path, context, token);

                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{folder.UserId}:{folder.ParentId}");
                context.Update(folder);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.MoveFolder - folder {folder.Name} moved successfully to {new_folder_path}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{folder.UserId}:{new_folder.ParentId}");
                LoggingController.LogDebug($"FileController.MoveFolder - Cleaned up cache for folder");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        private static async Task LoadFolderChildrenRecursive(Data.Entities.File folder, HostingDbContext context, CancellationToken token)
        {
            await context.Entry(folder).Collection(f => f.Children).LoadAsync(token);

            foreach (var child in folder.Children.Where(f => f.IsDirectory)) 
            {
                await LoadFolderChildrenRecursive(child, context, token);
            }
        }

        private static async Task UpdateFolderPathsRecursive(Data.Entities.File folder, string newFolderPath, HostingDbContext context, CancellationToken token)
        {
            foreach (var file in folder.Children)
            {
                string new_path = Path.Combine(newFolderPath, file.Name);
                file.Path = new_path;

                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{file.UserId}:{file.ParentId}");
                context.Update(file);
                LoggingController.LogInfo($"FileController.MoveFolder - file {file.Id} {file.Name} moved to {new_path}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{file.UserId}:{folder.ParentId}");
                LoggingController.LogDebug($"FileController.MoveFolder - Cleaned up cache for file {file.Id}");

                if (file.IsDirectory) 
                {
                    await UpdateFolderPathsRecursive(file, new_path, context, token);
                }
            }
        }

        public static async Task UpdateFolderPublicity(Data.Entities.File folder, bool publicity, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();

                await context.Entry(folder).Collection(f => f.Children).LoadAsync(token);

                folder.IsPublic = publicity;

                await UpdateChildPublicityAsync(folder.Children, publicity, context, token);

                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.UpdateFolderPublicity - folder {folder.Name} publicity updated");

                await CachedDataController.RemoveCacheAsync($"{CachePrefix}public:{folder.UserId}");
                LoggingController.LogDebug($"FileController.UpdateFolderPublicity - Cleaned up cache");
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

                await context.Entry(folder).Collection(f => f.Children).LoadAsync(token);
                string new_folder_path = Path.Combine(StoragePath, "Deleted", folder.Name);

                LoggingController.LogDebug($"FileController.DeleteFolder - moving folder {folder.Name} to {new_folder_path}");
                Directory.Move(folder.Path, new_folder_path);
                folder.Path = new_folder_path;
                folder.IsDeleted = true;
                 
                User user = await UserController.GetUserByIdAsync(folder.UserId, token);

                await DeleteChildren(folder.Children, new_folder_path, context, user, token);

                context.Update(folder);
                await context.SaveChangesAsync(token);
                
                await CachedDataController.ScheduleFileDeletionAsync(folder.Id.ToString(), user.AutoFileDeletionTime);

                LoggingController.LogInfo($"FileController.DeleteFolder - folder {folder.Name} moved to deleted successfully");

                await CachedDataController.RemoveCacheAsync($"{CachePrefix}all:{folder.UserId}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{folder.UserId}:{folder.ParentId}");
                LoggingController.LogDebug($"FileController.DeleteFolder - Cleaned up cache");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }
        public static async Task DeleteChildren(ICollection<Data.Entities.File> children, string path, HostingDbContext context, User user, CancellationToken token)
        {
            foreach (Data.Entities.File child in children)
            {
                token.ThrowIfCancellationRequested();

                string new_path = Path.Combine(path, child.Name);
                child.IsDeleted = true;
                Directory.Move(child.Path, new_path);
                child.Path = new_path;
                context.Files.Update(child);
                await CachedDataController.ScheduleFileDeletionAsync(child.Id.ToString(), user.AutoFileDeletionTime);


                LoggingController.LogInfo($"FileController.DeleteChildren - file {child.Id} {child.Name} marked as deleted and moved to {child.Path}");

                if (child.IsDirectory)
                {
                    await context.Entry(child).Collection(f => f.Children).LoadAsync(token);
                    await DeleteChildren(child.Children, child.Path, context, user, token);
                }
            }
        }

        public static async Task EraseFolder(Data.Entities.File folder, CancellationToken token)
        {
            using HostingDbContext context = new();
            try
            {
                token.ThrowIfCancellationRequested();

                await context.Entry(folder).Collection(f => f.Children).LoadAsync(token);
                LoggingController.LogDebug($"FileController.EraseFolder - erasing folder {folder.Name} at {folder.Path}");

                try
                {
                    Directory.Delete(folder.Path, true);
                }
                catch (Exception ex) when (ex is DirectoryNotFoundException || ex is IOException || ex is UnauthorizedAccessException)
                {
                    LoggingController.LogError($"FileController.EraseFolder - Failed to delete folder from file system: {ex.Message}");
                    throw;
                }

                await EraseChildren(folder.Children, context, token);

                context.Files.Remove(folder);
                await context.SaveChangesAsync(token);

                LoggingController.LogInfo($"FileController.EraseFolder - folder {folder.Name} and its children erased successfully");

                await CachedDataController.RemoveCacheAsync($"{UserController.CachePrefix}space:{folder.UserId}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}all:{folder.UserId}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{folder.UserId} : {folder.ParentId}");
                LoggingController.LogDebug($"FileController.EraseFolder - Cleaned up cache");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }
        public static async Task EraseChildren(ICollection<Data.Entities.File> children, HostingDbContext context, CancellationToken token)
        {
            foreach (Data.Entities.File child in children)
            {
                token.ThrowIfCancellationRequested();

                if (child.IsDirectory)
                {
                    await context.Entry(child).Collection(f => f.Children).LoadAsync(token);
                    await EraseChildren(child.Children, context, token);
                    context.Files.Remove(child);
                    LoggingController.LogInfo($"FileController.EraseChildren - Folder {child.Name} {child.Path} erased");
                }
                else
                {
                    context.Files.Remove(child);
                    LoggingController.LogInfo($"FileController.EraseChildren - File {child.Name} {child.Path} erased");
                }
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

                await CachedDataController.RemoveCacheAsync($"{UserController.CachePrefix}space:{folder.UserId}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}all:{folder.UserId}");
                await CachedDataController.RemoveCacheAsync($"{CachePrefix}{folder.UserId} : {folder.ParentId}");
                LoggingController.LogDebug($"FileController.EraseFolder - Cleaned up cache");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        #endregion
    }
}
