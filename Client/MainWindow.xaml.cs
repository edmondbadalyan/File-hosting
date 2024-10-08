﻿using System.Net.Sockets;
using System.Windows;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        LoginWindow login;
        RegisterWindow register;
        public static TcpClient Server { get; set; }
        public ConfigController Config { get; set; }

        public MainWindow() {
            InitializeComponent();
            Preset();
        }

        public async void Preset() {
            Config = new ConfigController();
            await Task.Delay(1000);
            Server = new(Config.IP, Config.Port);
        }

        private void Button_Login(object sender, RoutedEventArgs e) {
            login = new LoginWindow(this, new());
            this.Visibility = Visibility.Hidden;
            login.ShowDialog();
        }

        private void Button_Register(object sender, RoutedEventArgs e) {
            register = new RegisterWindow(this, new());
            this.Visibility = Visibility.Hidden;
            register.ShowDialog();
        }

        public void GoBack(System.Windows.Window window) {
            Visibility = Visibility.Visible;
            if (login is not null && login != window) login.Close();
            else if (register is not null && register != window) register.Close();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (login is not null) login.Close();
            else if (register is not null) register.Close();
            await Task.Run(async () => await ClientCommands.CloseConnectionAsync(Server));
        }
    }
}