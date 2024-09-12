using HostingLib.Data.Entities;
using System.Net.Sockets;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client {
    public class MainMenuWindowModel : BindableBase{
        public User User { get; set; }
        public TcpClient Client { get; set; }

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

        private int? selectedFolderId;
        public int? SelectedFolderId {
            get => selectedFolderId;
            set => SetProperty(ref selectedFolderId, value);
        }

        public MainMenuWindowModel(User user, TcpClient client) {
            User = user;
            Client = client;
            Files = new List<FileModel>();
            AllFiles = new List<File>();
            Task.Run(() => Update());
            Search = "";
            SelectedFolderId = null;
        }

        public async Task Update() {
            AllFiles = (List<File>) await Task.Run(async () => await ClientCommands.GetAllFilesAsync(Client, User));
            Files = AllFiles.Where(File => File.ParentId == SelectedFolderId && !File.IsDeleted).Select((File) => new FileModel(File)).ToList();
        }
    }
}
