using HostingLib.Data.Entities;
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
    /// Логика взаимодействия для MoveWindow.xaml
    /// </summary>
    public partial class MoveWindow : Window {
        MainWindow mainWindow;
        MoveWindowModel Model { get; set; }

        public MoveWindow(MainWindow mainWindow, MoveWindowModel model) {
            InitializeComponent();

            this.mainWindow = mainWindow;
            Model = model;

            DataContext = Model;
        }

        private async void Button_Send(object sender, RoutedEventArgs e) {
            File? folder_to = await Task.Run(async () => await ClientCommands.GetFileAsync(Model.Client, Model.Path, Model.User));
            if (folder_to is null) return;
            foreach (File file in Model.Files) {
                if (file.IsDirectory)
                    await Task.Run(async () => await ClientCommands.MoveFolderAsync(Model.Client, file, folder_to));
                else
                    await Task.Run(async () => await ClientCommands.MoveFileAsync(Model.Client, file, folder_to));
            }

            Close();
        }
    }
}
