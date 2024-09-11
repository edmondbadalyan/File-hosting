using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    public class CreateWindowModel : BindableBase {
        private string folderName = string.Empty;
        public string FolderName {
            get => folderName;
            set => SetProperty(ref folderName, value);
        }

        public string FolderPath { get; set; }

        public CreateWindowModel(string folderPath) {
            FolderPath = folderPath;
        }
    }
}
