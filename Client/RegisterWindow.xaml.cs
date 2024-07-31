using Microsoft.Win32;
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

        private void Button_Terms(object sender, RoutedEventArgs e) {
            MessageBox.Show(Model.Terms);
        }

        private void Button_Send(object sender, RoutedEventArgs e) {
            if (Validation.ValidateEmail(Model.Email) && Validation.ValidatePassword(PasswordBox)) {
                EmailCheckWindow window = new EmailCheckWindow(mainWindow, this, new(Model.Email));
                this.Visibility = Visibility.Hidden;
                window.ShowDialog();

                // отправка на сервер
            }
        }

        private void PasswordChanged(object sender, RoutedEventArgs e) {
            PasswordBox = (PasswordBox) sender;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}
