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

        private string folderPath;
        public string FolderPath {
            get => folderPath;
            set => SetProperty(ref folderPath, value);
        }

        public MainMenuWindowModel(User user, TcpClient client, List<FileModel> files, string folderPath) {
            User = user;
            Client = client;
            Files = files;
            FolderPath = folderPath;
        }

        public MainMenuWindowModel(User user, TcpClient client) {
            User = user;
            Client = client;
            Files = new List<FileModel>();
            Task.Run(() => Update());
            FolderPath = "";
        }

        public async Task Update() {
            Files = (await Task.Run(async () => await ClientCommands.GetAllFilesAsync(Client, User))).Select((File) => new FileModel(File)).ToList();
        }
    }
}
