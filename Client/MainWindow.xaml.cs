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
        public MainWindow() {
            InitializeComponent();
        }

        private void Button_Login(object sender, RoutedEventArgs e) {

        }

        private void Button_Register(object sender, RoutedEventArgs e) {

        }

        public void GoBack(System.Windows.Window window) {
            Visibility = Visibility.Visible;
            if (create is not null && create != window) create.Close();
            else if (lobby is not null && lobby != window) lobby.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (create is not null) create.Close();
            else if (lobby is not null) lobby.Close();
        }
    }
}