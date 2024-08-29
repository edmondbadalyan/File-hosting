using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
            await Task.Run(async () => await ClientCommands.CreateFolderAsync(mainWindow.Server, Model.FolderName, Model.User, Model.ParentFolder, Model.IsPublic));
        }
    }
}
