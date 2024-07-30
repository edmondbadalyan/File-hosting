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
    /// Логика взаимодействия для RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        MainWindow mainWindow;
        RegisterWindowModel Model { get; set; }

        public RegisterWindow(MainWindow mainWindow, RegisterWindowModel model) {
            InitializeComponent();

            this.mainWindow = mainWindow;
            Model = model;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}
