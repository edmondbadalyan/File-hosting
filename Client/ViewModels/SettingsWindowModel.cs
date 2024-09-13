using Client.Services;
using HostingLib.Data.Entities;
using System.Net.Sockets;
using System.Windows;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client
{
    public class SettingsWindowModel : BindableBase
    {
        public readonly UserSingleton user_singleton;

        private bool isInitializing; 

        private bool publicity;
        public bool Publicity
        {
            get => publicity;
            set
            {
                if (SetProperty(ref publicity, value))
                {
                    PublicityString = value ? "Публичный" : "Приватный";
                    if (!isInitializing)
                    {
                        UpdatePublicity(value);
                    }
                }
            }
        }

        private string publicityString;
        public string PublicityString
        {
            get => publicityString;
            set
            {
                SetProperty(ref publicityString, value);
            }
        }

        private string space;
        public string Space
        {
            get => space;
            set => SetProperty(ref space, value);
        }

        private TimeSpan publicityTimeout;
        public TimeSpan PublicityTimeout
        {
            get => publicityTimeout;
            set
            {
                if (SetProperty(ref publicityTimeout, value) && !isInitializing)
                {
                    UpdateAutoDeletionTime(value);
                }
            }
        }

        public SettingsWindowModel(User user, TcpClient client)
        {
            isInitializing = true;
            user_singleton = UserSingleton.GetInstance();
            Publicity = user.IsPublic;
            PublicityString = publicity ? "Публичный" : "Приватный";
            UpdateSpace();
            isInitializing = false; 
        }

        public async void UpdateSpace()
        {
            Space = Utilities.FormatBytes(await Task.Run(async () => await ClientCommands.GetAvailableSpaceAsync(user_singleton.Client, user_singleton.User)));
            PublicityTimeout = await Task.Run(async () => await ClientCommands.GetAutoDeletionTimeAsync(user_singleton.Client, user_singleton.User));
        }

        public async void UpdatePublicity(bool publicity)
        {
            await Task.Run(async () => await user_singleton.UpdateUserPublicity(publicity));
        }

        public async void UpdateAutoDeletionTime(TimeSpan time)
        {
            await Task.Run(async () => await user_singleton.UpdateAutoDeletionTime(time));
        }
    }
}
