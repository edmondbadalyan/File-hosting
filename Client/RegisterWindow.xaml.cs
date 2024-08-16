using System.Windows;
using System.Windows.Controls;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client {
    /// <summary>
    /// Логика взаимодействия для RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        MainWindow mainWindow;
        RegisterWindowModel Model { get; set; }
        PasswordBox PasswordBox { get; set; }

        public RegisterWindow(MainWindow mainWindow, RegisterWindowModel model) {
            InitializeComponent();

            this.mainWindow = mainWindow;
            Model = model;

            DataContext = Model;
        }        

        private void Button_Terms(object sender, RoutedEventArgs e) {
            MessageBox.Show(Model.Terms);
        }

        private async void Button_Send(object sender, RoutedEventArgs e) {
            if (Utilities.ValidateEmail(Model.Email) && Utilities.ValidatePassword(PasswordBox)) {
                await Task.Run(async () => await ClientCommands.CreateUserAsync(mainWindow.Server, Model.Email, PasswordBox.Password));

                EmailCheckWindow window = new EmailCheckWindow(mainWindow, this, new(Model.Email));
                this.Visibility = Visibility.Hidden;
                window.ShowDialog();
            }
        }

        private void PasswordChanged(object sender, RoutedEventArgs e) {
            PasswordBox = (PasswordBox) sender;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}
