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

        private string search;
        public string Search {
            get => search;
            set => SetProperty(ref search, value);
        }

        private FileModel selectedFolder;
        public FileModel SelectedFolder {
            get => selectedFolder;
            set => SetProperty(ref selectedFolder, value);
        }

        public MainMenuWindowModel(User user, TcpClient client, List<FileModel> files, string search, FileModel selectedFolder) {
            User = user;
            Client = client;
            Files = files;
            Search = search;
            SelectedFolder = selectedFolder;
        }

        public MainMenuWindowModel(User user, TcpClient client) {
            User = user;
            Client = client;
            Files = new List<FileModel>();
            Task.Run(() => Update());
            Search = "";
            SelectedFolder = null;
        }

        public async Task Update() {
            Files = (await Task.Run(async () => await ClientCommands.GetAllFilesAsync(Client, User, (SelectedFolder.File.Id == null ? -1 : SelectedFolder.File.Id) ))).Select((File) => new FileModel(File)).ToList();
        }
    }
}
