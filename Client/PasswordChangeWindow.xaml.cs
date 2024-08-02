using System.Windows;
using System.Windows.Controls;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client {
    /// <summary>
    /// Логика взаимодействия для PasswordChangeWindow.xaml
    /// </summary>
    public partial class PasswordChangeWindow : Window
    {
        MainWindow mainWindow;
        string email;
        PasswordBox Password { get; set; }
        PasswordBox PasswordCopy { get; set; }

        public PasswordChangeWindow(MainWindow mainWindow, object previous, string email) {
            InitializeComponent();

            this.mainWindow = mainWindow;
            this.email = email;
        }

        private void PasswordChanged(object sender, RoutedEventArgs e) {
            Password = (PasswordBox)sender;
        }

        private void PasswordCopyChanged(object sender, RoutedEventArgs e) {
            PasswordCopy = (PasswordBox)sender;
        }

        private async void Button_Send(object sender, RoutedEventArgs e) {
            if (Validation.ValidatePassword(Password) && Validation.ValidatePassword(PasswordCopy) && Password.Password == PasswordCopy.Password) {
                await ClientCommands.UpdateUserAsync(
                    mainWindow.Server, 
                    await ClientCommands.GetUserAsync(mainWindow.Server, email, null),
                    Password.Password
                );

                mainWindow.GoBack(this);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}
