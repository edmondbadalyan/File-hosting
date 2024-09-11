using HostingLib.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client;
public class MoveWindowModel : BindableBase {
    public User User { get; set; }
    public TcpClient Client { get; set; }
    public IReadOnlyList<File> Files { get; set; }

    private string path;
    public string Path {
        get => path;
        set => SetProperty(ref path, value);
    }

    private List<string> allPaths;
    public List<string> AllPaths {
        get => allPaths;
        set => SetProperty(ref allPaths, value);
    }

    public MoveWindowModel(User user, TcpClient client, IReadOnlyList<File> files, List<string> allPaths) {
        User = user;
        Client = client;
        Files = files;
        Path = "";
        AllPaths = allPaths;
    }
}
