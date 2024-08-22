using HostingLib.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client {
    public class SettingsWindowModel {
        public User User { get; set; }

        private bool isDark;
        public bool IsDark {
            get => isDark;
            set {
                if (isDark == value)
                    return;
                isDark = value;

                string style = value ? "dark" : "light";
                Uri uri = new Uri("Themes\\" + style + ".xaml", UriKind.Relative);
                ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
        }

        public SettingsWindowModel(User user) {
            User = user;
        }
    }
}
