using HostingLib.Data.Entities;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    public class MainMenuWindowModel : BindableBase{
        public User User { get; set; }

        private List<FileModel> files;
        public List<FileModel> Files {
            get => files;
            set => SetProperty(ref files, value);
        }

        private string folderPath;
        public string FolderPath {
            get => folderPath;
            set => SetProperty(ref folderPath, value);
        }

        public MainMenuWindowModel(User user, List<FileModel> files, string folderPath) {
            User = user;
            Files = files;
            FolderPath = folderPath;
        }

        public MainMenuWindowModel(User user) {
            User = user;
            Files = new List<FileModel>();
            Update();
            FolderPath = "";
        }

        public async Task Update() {
            // здесь будет запрос на обновление списка файлов
        }
    }
}
