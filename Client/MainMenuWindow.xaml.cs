using HostingLib.Classes;
using HostingLib.Controllers;
using HostingLib.Data.Entities;
using Microsoft.Win32;
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
    /// Логика взаимодействия для MainMenuWindow.xaml
    /// </summary>
    public partial class MainMenuWindow : Window {
        MainWindow mainWindow;
        MainMenuWindowModel Model { get; set; }

        public MainMenuWindow(MainWindow mainWindow, MainMenuWindowModel mainMenuWindowModel) {
            InitializeComponent();

            this.mainWindow = mainWindow;
            Model = mainMenuWindowModel;

            DataContext = Model;
        }

        private async void Button_Download(object sender, RoutedEventArgs e) {            
            string whereto = string.Empty;
            OpenFolderDialog folderDialog = new OpenFolderDialog();
            if (folderDialog.ShowDialog() == true) {
                whereto = folderDialog.FolderName;
            }

            IReadOnlyList<File> files = dg.SelectedItems.Cast<FileModel>().Select(FileModel => FileModel.File).ToArray();
            foreach (File file in files) {
                await Task.Run(async () => await ClientCommands.DownloadFileAsync(Model.Client, System.IO.Path.Combine(whereto, file.Name), file, Model.User));
            }

            await Model.Update();
        }

        private async void Button_Upload(object sender, RoutedEventArgs e) {
            List<string> wherefrom = new List<string>();
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true) {
                wherefrom = fileDialog.FileNames.ToList();
            }

            foreach (string file in wherefrom) {
                await Task.Run(async () => await ClientCommands.UploadFileAsync(Model.Client, file, Model.User, Model.SelectedFolder.File));
            }

            await Model.Update();
        }

        private void Button_Create(object sender, RoutedEventArgs e) {
            CreateWindow window = new CreateWindow(mainWindow, new(Model.Search));
            window.ShowDialog();
        }

        private async void Button_Delete(object sender, RoutedEventArgs e) {
            IReadOnlyList<File> files = dg.SelectedItems.Cast<FileModel>().Select(FileModel => FileModel.File).ToArray();
            foreach (File file in files) {
                await Task.Run(async () => await ClientCommands.DeleteFileAsync(Model.Client, file));
            }

            await Model.Update();
        }

        private async void Button_DeletedFiles(object sender, RoutedEventArgs e) {
            // здесь будет запрос на получение файлов из папки "удалённых"
        }

        private void Button_Settings(object sender, RoutedEventArgs e) {
            SettingsWindow window = new SettingsWindow(mainWindow);
            window.ShowDialog();
        }

        private void Button_Open(object sender, RoutedEventArgs e) {
            if (dg.SelectedItems.Count == 1) {
                FileModel file = dg.SelectedItems.Cast<FileModel>().First();
                if (file.Extension == "") {
                    Model.SelectedFolder = file;
                }
                else {
                    // предпросмотр
                }
            }
        }

        private void Button_Exit(object sender, RoutedEventArgs e) => Close();

        private async void Button_Search(object sender, RoutedEventArgs e) {
            if (Model.Search == "" || !System.IO.Path.Exists(Model.Search)) {
                System.Windows.Forms.MessageBox.Show("Введите путь");
                return;
            }

            Model.SelectedFolder = new(await Task.Run(async () => await ClientCommands.GetFileAsync(Model.Client, Model.Search, Model.User)));
            await Model.Update();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}
