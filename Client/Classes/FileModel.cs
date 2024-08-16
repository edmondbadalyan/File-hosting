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
        public string FileName { get; set; }
        public DateTime LastChangeDate { get; set; }
        public string Weight { get; set; }
        public string Extension { get; set; }

        public FileModel(File file) {
            File = file;
            FileName = System.IO.Path.GetFileName(File.Path);

            LastChangeDate = File.ChangeDate;
            Weight = Utilities.FormatBytes(File.Size);

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
