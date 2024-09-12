using HostingLib.Data.Entities;
using ClientCommands = HostingLib.Сlient.Client;

namespace Client {
    public class FileModel : BindableBase
    {
        public File File { get; set; }
        public string FileName { get; set; }
        public DateTime LastChangeDate { get; set; }
        public string Weight { get; set; }
        public string Extension { get; set; }

        private bool isPublic;
        public bool IsPublic {
            get => isPublic;
            set {
                SetProperty(ref isPublic, value);
                Update(value);
            }
        }

        public FileModel(File file) {
            File = file;
            FileName = System.IO.Path.GetFileName(File.Path);

            LastChangeDate = File.ChangeDate;
            Weight = Utilities.FormatBytes(File.Size);
            isPublic = File.IsPublic;

            if (File.Path.EndsWith("\\")) {
                Extension = "";
            }
            else {
                Extension = System.IO.Path.GetExtension(File.Path);
                if (Extension == "") Extension = ".*";
            }
        }

        private async void Update(bool publicity) {
            await Task.Run(async () => await ClientCommands.UpdateFilePublicity(MainWindow.Server, File, publicity));
        }
    }
}
