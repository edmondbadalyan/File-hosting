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

        private bool isDark;
        public bool IsDark {
            get => isDark;
            set {
                if (isDark == value)
                    return;
                isDark = value;

                string style = value ? "dark" : "light";
                Uri uri = new Uri("Themes\\" + style + ".xaml", UriKind.Relative);
                ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
        }

        public SettingsWindowModel(User user, TcpClient client) {
            User = user;
            Client = client;
            Publicity = user.IsPublic;
            UpdateSpace();
            IsDark = false;
        }

        public async void UpdateSpace () =>
            Utilities.FormatBytes(await Task.Run(async () => await ClientCommands.GetAvailableSpaceAsync(Client, User)));
    }
}
