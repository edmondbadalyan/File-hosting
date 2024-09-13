using HostingLib.Data.Entities;
using Microsoft.Win32;
using System.Windows;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client {
    /// <summary>
    /// Логика взаимодействия для PublicFilesWindow.xaml
    /// </summary>
    public partial class PublicFilesWindow : Window {
        MainMenuWindow menuWindow;
        PublicFilesWindowModel Model { get; set; }

        public PublicFilesWindow(MainMenuWindow menuWindow, PublicFilesWindowModel model) {
            InitializeComponent();

            this.menuWindow = menuWindow;
            Model = model;

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
                await Task.Run(async () => await ClientCommands.DownloadFileAsync(Model.user_singleton.Client, System.IO.Path.Combine(whereto, file.Name), file, Model.user_singleton.User));
            }

            await Model.Update();
        }

        private async void Button_Open(object sender, RoutedEventArgs e) {
            if (dg.SelectedItems.Count == 1) {
                FileModel file = dg.SelectedItems.Cast<FileModel>().First();
                if (file.Extension == ".*") {
                    Model.SelectedFolderId = file.File.Id;
                    await Model.Update();
                }
                else {
                    // предпросмотр
                }
            }
        }

        private async void Button_Search(object sender, RoutedEventArgs e) {
            if (Model.Search == "" || !System.IO.Path.Exists(Model.Search)) {
                System.Windows.Forms.MessageBox.Show("Введите путь");
                return;
            }
            File? file = Model.AllFiles.FirstOrDefault(File => File.Path == Model.Search);
            if (file is null) {
                System.Windows.Forms.MessageBox.Show("Некорректный путь");
                return;
            }

            if (file.IsDirectory) {
                Model.SelectedFolderId = file.Id;
                await Model.Update();
            }
            else {
                // предпросмотр
            }
        }

        private async void Button_UserSearch(object sender, RoutedEventArgs e) {
            if (Model.UserSearch == "" || !Utilities.ValidateEmail(Model.UserSearch)) {
                System.Windows.Forms.MessageBox.Show("Введите email пользователя");
                return;
            }
            User? user = await Task.Run(async () => await ClientCommands.GetUserAsync(Model.user_singleton.Client, Model.UserSearch));
            if (user is null) {
                System.Windows.Forms.MessageBox.Show("Данный пользователь не существует");
                return;
            }
            if (!user.IsPublic) {
                System.Windows.Forms.MessageBox.Show("Данный пользователь закрыл доступ к своим файлам");
                return;
            }
            Model.FoundUser = user;
            await Model.Update();
        }

        private void Button_Exit(object sender, RoutedEventArgs e) => Close();

        private async void Button_Back(object sender, RoutedEventArgs e) {
            int? id = Model.AllFiles.First(File => File.Id == Model.SelectedFolderId).ParentId;
            if (id.HasValue) Model.SelectedFolderId = id.Value;
            else Model.SelectedFolderId = null;
            await Model.Update();
        }

        private async void Button_Root(object sender, RoutedEventArgs e) {
            Model.SelectedFolderId = null;
            await Model.Update();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => menuWindow.Visibility = Visibility.Visible;

    }
}
