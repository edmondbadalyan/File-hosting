using System.Windows;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client {
    /// <summary>
    /// Логика взаимодействия для CreateWindow.xaml
    /// </summary>
    public partial class CreateWindow : Window {
        MainWindow mainWindow;
        CreateWindowModel Model { get; set; }

        public CreateWindow(MainWindow mainWindow, CreateWindowModel model) {
            InitializeComponent();

            this.mainWindow = mainWindow;
            Model = model;
            DataContext = Model;
        }

        private async void Button_Create(object sender, RoutedEventArgs e) {
            await Task.Run(async () => await ClientCommands.CreateFolderAsync(MainWindow.Server, Model.FolderName, Model.user_singleton.User, Model.ParentFolder, Model.IsPublic));
            Close();
        }
    }
}
