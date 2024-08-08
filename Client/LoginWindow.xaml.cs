using HostingLib.Data.Entities;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client {
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        MainWindow mainWindow;
        LoginWindowModel Model { get; set; }
        PasswordBox PasswordBox { get; set; }

        public LoginWindow(MainWindow mainWindow, LoginWindowModel model) {
            InitializeComponent();

            this.mainWindow = mainWindow;
            Model = model;

            DataContext = Model;
        }

        private bool CheckEmail() {
            if (Model.Email is null || Model.Email.Length == 0) {
                MessageBox.Show("Input email");
                return false;
            }
            return true;
        }
        private bool CheckPassword() {
            if (PasswordBox is null || PasswordBox.Password.Length == 0) {
                MessageBox.Show("Input password");
                return false;
            }
            return true;
        }

        private void Button_ForgotPassword(object sender, RoutedEventArgs e) {
            if (!CheckEmail()) return;

            EmailCheckWindow window = new EmailCheckWindow(mainWindow, this, new(Model.Email));
            this.Visibility = Visibility.Hidden;
            window.ShowDialog();
        }

        private async void Button_Login(object sender, RoutedEventArgs e) {
            if (!CheckEmail() || !CheckPassword()) return;

            User? user = await Task.Run(async () => await ClientCommands.GetUserAsync(mainWindow.Server, Model.Email, PasswordBox.Password));

            if (user is null) {
                MessageBox.Show("Incorrect data");
                return;
            }

            MainMenuWindow window = new MainMenuWindow(mainWindow, new(user));
            this.Visibility = Visibility.Hidden;
            window.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);

        private void PasswordChanged(object sender, RoutedEventArgs e) {
            PasswordBox = (PasswordBox)sender;
        }
    }
}
