using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        LoginWindow login;
        RegisterWindow register;
        public TcpClient Server { get; set; }

        public MainWindow() {
            InitializeComponent();
            Server = new("192.168.1.133", 2024);
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (login is not null) login.Close();
            else if (register is not null) register.Close();
            Server.Dispose();
        }
    }
}