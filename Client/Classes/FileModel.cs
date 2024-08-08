using HostingLib.Data.Entities;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class FileModel : BindableBase
    {
        public File File { get; set; }
        public string Extension { get; set; }

        public bool isSelected;
        public bool IsSelected {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        public FileModel(File file) {
            File = file;
            IsSelected = false;

            if (File.Path.EndsWith("\\")) {
                Extension = "";
            }
            else {
                Extension = System.IO.Path.GetExtension(File.Path);
                if (Extension == "") Extension = ".*";
            }
        }
    }
}
