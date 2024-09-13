using System.Windows;
using ClientCommands = HostingLib.Сlient.Client;
using Xceed;

namespace Client {
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {
        MainWindow window;
        SettingsWindowModel Model { get; set; }

        public SettingsWindow(MainWindow window, SettingsWindowModel model) {
            InitializeComponent();
            this.window = window;
            Model = model;

            DataContext = Model;
        }

        private void Button_LightTheme(object sender, RoutedEventArgs e) => window.Config.Theme = "light";
        private void Button_DarkTheme(object sender, RoutedEventArgs e) => window.Config.Theme = "dark";

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            await Task.Run(async () => await ClientCommands.UpdateUserPublicityAsync(Model.user_singleton.Client, Model.user_singleton.User, Model.Publicity));
            await Task.Run(async () => await ClientCommands.UpdateUserFileDeletionTimeAsync(Model.user_singleton.Client, Model.user_singleton.User, Model.PublicityTimeout));
        }
    }
}
