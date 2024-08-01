using System;
using System.Collections.Generic;
using System.Linq;
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

        private void Button_Send(object sender, RoutedEventArgs e) {
            if (Validation.ValidatePassword(Password) && Validation.ValidatePassword(PasswordCopy) && Password.Password == PasswordCopy.Password) {
                // отправка на сервер

                mainWindow.GoBack(this);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}
