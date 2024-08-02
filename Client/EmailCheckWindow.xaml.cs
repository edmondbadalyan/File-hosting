using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
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

namespace Client
{
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

            using SmtpClient smtpClient = new SmtpClient("smtp.mail.ru");
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
                    // пускаем в приложение
                }
            }
            else {
                MessageBox.Show("Некорректный код");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Model.IsFromRegistration && Model.Code != Model.CodeInput) {
                // удаляем из бд данные об этом пользователе
            }
            mainWindow.GoBack(this);
        }
    }
}
