using HostingLib.Data.Entities;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
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

        public MainMenuWindowModel(User user, TcpClient client, List<FileModel> files, string search, int selectedFolderId) {
            User = user;
            Client = client;
            Files = files;
            Search = search;
            SelectedFolderId = selectedFolderId;
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
            Files = AllFiles.Where(File => File.ParentId == SelectedFolderId).Select((File) => new FileModel(File)).ToList();
        }
    }
}
