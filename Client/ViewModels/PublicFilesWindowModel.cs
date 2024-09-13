using Client.Services;
using HostingLib.Data.Entities;
using System.Net.Sockets;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client {
    public class PublicFilesWindowModel : BindableBase {
        public readonly UserSingleton user_singleton;

        private List<FileModel> files;
        public List<FileModel> Files {
            get => files;
            set => SetProperty(ref files, value);
        }

        private List<File> allFiles;
        public List<File> AllFiles {
            get => allFiles;
            set => SetProperty(ref allFiles, value);
        }

        public IReadOnlyList<string> AllFilesString {
            get => allFiles.Select(File => File.Path).ToArray();
        }

        private string search;
        public string Search {
            get => search;
            set => SetProperty(ref search, value);
        }

        private string userSearch;
        public string UserSearch {
            get => userSearch;
            set => SetProperty(ref userSearch, value);
        }
        public User FoundUser;

        private int? selectedFolderId;
        public int? SelectedFolderId {
            get => selectedFolderId;
            set => SetProperty(ref selectedFolderId, value);
        }

        public PublicFilesWindowModel(User user, TcpClient client) {
            user_singleton = UserSingleton.GetInstance();
            Files = new List<FileModel>();
            AllFiles = new List<File>();
            Task.Run(() => Update());
            Search = "";
            UserSearch = "";
            FoundUser = null;
            SelectedFolderId = null;
        }

        public async Task Update() {
            if (FoundUser is null) return;
            AllFiles = (List<File>)await Task.Run(async () => await ClientCommands.GetPublicFilesAsync(user_singleton.Client, FoundUser));
            Files = AllFiles.Where(File => File.ParentId == SelectedFolderId).Select((File) => new FileModel(File)).ToList();
        }
    }
}
