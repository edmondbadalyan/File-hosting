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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        MainWindow mainWindow;
        LoginWindowModel Model { get; set; }

        public LoginWindow(MainWindow mainWindow, LoginWindowModel model) {
            InitializeComponent();

            this.mainWindow = mainWindow;
            Model = model;
        }

        private bool CheckEmail() {
            if (Model.Email is null || Model.Email.Length == 0) {
                MessageBox.Show("Input email");
                return false;
            }
            // if (проверка существования пользователя через сервер) { 
            //     MessageBox.Show("Incorrect email");
            //     return false;
            // }
            return true;
        }
        private bool CheckPassword() {
            // if (проверка корректности пароля через сервер) { 
            //     MessageBox.Show("Incorrect password");
            //     return false;
            // }
            return true;
        }

        private void Button_ForgotPassword(object sender, RoutedEventArgs e) {
            if (!CheckEmail()) return;

            // переход в окно смены пароля
        }

        private void Button_Login(object sender, RoutedEventArgs e) {
            if (!CheckEmail()) return;
            if (!CheckPassword()) return;

            // переход в главное меню
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}
