using System.IO;
using System.Windows;

namespace Client {
    public class ConfigController {
        private string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + @"\Config\config.json";
        private Config config;
        
        public string IP { get; private set; }
        public int Port { get; private set; }

        public string Theme {
            get => config.Theme;
            set {
                config.Theme = value;

                WriteConfig();

                Uri uri = new Uri("Themes\\" + Theme + ".xaml", UriKind.Relative);
                ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
        }

        public ConfigController() {
            ReadConfig();
        }

        private async void ReadConfig() {
            try {
                StreamReader reader = new StreamReader(path);
                config = await Task.Run(async () => Config.FromJson(await reader.ReadToEndAsync()));
                reader.Close();
                if (config is null) throw new ArgumentNullException("Config is null");
                Theme = config.Theme;
                IP = config.Ip;
                Port = (int)config.Port;
            }
            catch (ArgumentNullException ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private async void WriteConfig() {
            StreamWriter writer = new StreamWriter(path);
            await Task.Run(async () => await writer.WriteAsync(config.ToJson()));
            writer.Close();
        }
    }
}
