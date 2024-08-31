using System.Windows;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client {
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {
        SettingsWindowModel Model { get; set; }

        public SettingsWindow(SettingsWindowModel model) {
            InitializeComponent();
            
            this.Model = model;
        }

        private void Button_LightTheme(object sender, RoutedEventArgs e) => Model.IsDark = false;
        private void Button_DarkTheme(object sender, RoutedEventArgs e) => Model.IsDark = true;

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            await Task.Run(async () => await ClientCommands.UpdateUserPublicityAsync(Model.Client, Model.User, Model.Publicity));
        }
    }
}
