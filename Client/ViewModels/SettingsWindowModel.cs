using HostingLib.Data.Entities;
using System.Net.Sockets;
using System.Windows;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client {
    public class SettingsWindowModel : BindableBase {
        public User User { get; set; }
        public TcpClient Client { get; set; }        

        private bool publicity;
        public bool Publicity {
            get => publicity;
            set {
                SetProperty(ref publicity, value);
                SetProperty(ref publicityString, value ? "Публичный" : "Приватный");
            }
        }
        private string publicityString;
        public string PublicityString {
            get => publicityString;
        }
        private string space;
        public string Space {
            get => space;
            set => SetProperty(ref space, value);
        }

        private TimeSpan publicityTimeout;
        public TimeSpan PublicityTimeout {
            get => publicityTimeout;
            set => SetProperty(ref publicityTimeout, value);
        }

        public SettingsWindowModel(User user, TcpClient client) {
            User = user;
            Client = client;
            Publicity = user.IsPublic;
            UpdateSpace();
        }

        public async void UpdateSpace() {
            Space = Utilities.FormatBytes(await Task.Run(async () => await ClientCommands.GetAvailableSpaceAsync(Client, User)));
            PublicityTimeout = await Task.Run(async () => await ClientCommands.GetAutoDeletionTimeAsync(Client, User));
        }
    }
}
