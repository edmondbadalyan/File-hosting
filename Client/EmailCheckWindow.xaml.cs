﻿using HostingLib.Data.Entities;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client {
    /// <summary>
    /// Логика взаимодействия для EmailCheckWindow.xaml
    /// </summary>
    public partial class EmailCheckWindow : Window
    {
        MainWindow mainWindow;
        EmailCheckWindowModel Model { get; set; }

        public EmailCheckWindow(MainWindow mainWindow, object previous, EmailCheckWindowModel model)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            Model = model;

            Model.IsFromLogin = previous is LoginWindow;
            Model.IsFromRegistration = previous is RegisterWindow;

            DataContext = Model;

            SendCode();
        }

        private async void SendCode () {
            Random rnd = new();

            MailAddress from = new MailAddress("p_recovery@inbox.ru");
            MailAddress to = new MailAddress(Model.Email);

            MailMessage message = new(from, to);
            string msg = "";
            for (int i = 0; i < 6; i++) {
                msg += rnd.Next(0, 9);
            }
            Model.Code = msg;

            message.Subject = "Подтвердите вашу почту";

            message.IsBodyHtml = true;

            using SmtpClient smtpClient = new SmtpClient("smtp.mail.ru", 587);
            smtpClient.Credentials = new NetworkCredential("p_recovery@inbox.ru", "g56ZgHrHERyVmG717v37");
            smtpClient.EnableSsl = true;
            message.Body = $@"<h1>Код: {msg}</h1>";

            await smtpClient.SendMailAsync(message);
        }

        private void Button_Send(object sender, RoutedEventArgs e) {
            if (Model.Code == Model.CodeInput) {
                if (Model.IsFromLogin) {
                    PasswordChangeWindow window = new PasswordChangeWindow(mainWindow, this, Model.Email);
                    this.Visibility = Visibility.Hidden;
                    window.ShowDialog();
                }
                else if (Model.IsFromRegistration) {
                    Model.IsFromRegistration = false;
                    mainWindow.GoBack(this);
                }
            }
            else {
                MessageBox.Show("Некорректный код");
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Model.IsFromRegistration && Model.Code != Model.CodeInput) {
                User? user = await Task.Run(async () => await ClientCommands.GetUserAsync(MainWindow.Server, Model.Email));

                if (user is not null) {
                    await Task.Run(async () => { await ClientCommands.DeleteUserAsync(MainWindow.Server, user); });
                }
            }
            mainWindow.GoBack(this);
        }

        private void PasswordChanged(object sender, RoutedEventArgs e) {
            Model.CodeInput = ((PasswordBox)sender).Password;
        }
    }
}
