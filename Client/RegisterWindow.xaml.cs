using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
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
        }

        public bool ValidateEmail() {
            try {
                MailAddress m = new MailAddress(Model.Email);
                return Model.Allowed = true;
            }
            catch (FormatException) {
                MessageBox.Show("Incorrect email");
                return Model.Allowed = false;
            }
        }

        private void ValidatePassword() {
            string ErrorMessage = string.Empty;

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (PasswordBox is null || string.IsNullOrWhiteSpace(PasswordBox.Password)) {
                ErrorMessage = "Password should not be empty";
                Model.Allowed = false;
            }
            else if (!hasLowerChar.IsMatch(PasswordBox.Password)) {
                ErrorMessage = "Password should contain At least one lower case letter";
                Model.Allowed = false;
            }
            else if (!hasUpperChar.IsMatch(PasswordBox.Password)) {
                ErrorMessage = "Password should contain At least one upper case letter";
                Model.Allowed = false;
            }
            else if (!hasMiniMaxChars.IsMatch(PasswordBox.Password)) {
                ErrorMessage = "Password should not be less than or greater than 12 characters";
                Model.Allowed = false;
            }
            else if (!hasNumber.IsMatch(PasswordBox.Password)) {
                ErrorMessage = "Password should contain At least one numeric value";
                Model.Allowed = false;
            }
            else if (!hasSymbols.IsMatch(PasswordBox.Password)) {
                ErrorMessage = "Password should contain At least one special case characters";
                Model.Allowed = false;
            }
            else {
                Model.Allowed = true;
            }

            if (ErrorMessage != string.Empty) {
                MessageBox.Show(ErrorMessage);
            }
        }

        private void Button_Terms(object sender, RoutedEventArgs e) {
            MessageBox.Show(Model.Terms);
        }

        private void Button_Send(object sender, RoutedEventArgs e) {
            ValidateEmail();
            ValidatePassword();
            // проверка подлиности почты
            // отправка на сервер
        }

        private void PasswordChanged(object sender, RoutedEventArgs e) {
            PasswordBox = (PasswordBox) sender;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}
