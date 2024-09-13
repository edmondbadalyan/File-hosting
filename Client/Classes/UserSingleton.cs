using HostingLib.Data.Entities;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client.Services
{
    public class UserSingleton
    {
        private static UserSingleton instance;
        private static readonly object _lock = new object();

        public User User { get; private set; }
        public TcpClient Client { get; private set; }

        private UserSingleton() { }

        public static UserSingleton GetInstance()
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new UserSingleton();
                    }
                }
            }
            return instance;
        }

        public void Initialize(User user, TcpClient client)
        {
            User = user;
            Client = client;
        }

        public async Task UpdateUserPublicity(bool publicity)
        {
            await Task.Run(async () => {
                await ClientCommands.UpdateUserPublicityAsync(Client, User, publicity);
                await RefreshUser(); 
            });
        }

        public async Task UpdateAutoDeletionTime(TimeSpan time)
        {
            await Task.Run(async () => {
                await ClientCommands.UpdateUserFileDeletionTimeAsync(Client, User, time);
                await RefreshUser(); 
            });
        }

        public async Task RefreshUser()
        {
            User = await ClientCommands.GetUserAsync(Client, User.Email);
        }
    }
}
