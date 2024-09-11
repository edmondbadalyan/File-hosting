using HostingLib.Data.Entities;
using System.Windows;
using System.Windows.Forms;
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
            Microsoft.Win32.OpenFolderDialog folderDialog = new Microsoft.Win32.OpenFolderDialog();
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
            List<string> fileNames = new List<string>();
            bool isPublic = false;
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            if (fileDialog.ShowDialog() == true) {
                fileNames = fileDialog.FileNames.ToList();
            }
            if (System.Windows.Forms.MessageBox.Show("Сделать файлы публичными?", "Сделать файлы публичными?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                isPublic = true;

            File? whereto = Model.AllFiles.FirstOrDefault(File => File.Id == Model.SelectedFolderId);
            if (whereto is null) return;
            foreach (string file in fileNames) {
                await Task.Run(async () => await ClientCommands.UploadFileAsync(Model.Client, file, Model.User, whereto, isPublic));
            }

            await Model.Update();
        }

        private void Button_Create(object sender, RoutedEventArgs e) {
            CreateWindow window = new CreateWindow(mainWindow, new(Model.AllFiles.First(File => File.Id == Model.SelectedFolderId), Model.User));
            window.ShowDialog();
        }

        private void Button_Move(object sender, RoutedEventArgs e) {
            IReadOnlyList<File> files = dg.SelectedItems.Cast<FileModel>().Select(FileModel => FileModel.File).ToArray();
            MoveWindow window = new MoveWindow(mainWindow, new(Model.User, Model.Client, files, Model.AllFilesString.Where(file => file[^1] == '\\').ToList()));
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
            await Task.Run (() => Model.Files = Model.AllFiles.Where(File => File.IsDeleted).Select((File) => new FileModel(File)).ToList());
        }

        private void Button_Settings(object sender, RoutedEventArgs e) {
            SettingsWindow window = new SettingsWindow(mainWindow, new (Model.User, Model.Client));
            window.ShowDialog();
        }

        private void Button_Open(object sender, RoutedEventArgs e) {
            if (dg.SelectedItems.Count == 1) {
                FileModel file = dg.SelectedItems.Cast<FileModel>().First();
                if (file.Extension == "") {
                    Model.SelectedFolderId = file.File.Id;
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

        private void Button_Others(object sender, RoutedEventArgs e) {
            PublicFilesWindow window = new PublicFilesWindow(this, new(Model.User, Model.Client));
            this.Visibility = Visibility.Hidden;
            window.ShowDialog();
        }

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

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            await Model.UpdatePublicity();
            mainWindow.GoBack(this);
        }
    }
}
