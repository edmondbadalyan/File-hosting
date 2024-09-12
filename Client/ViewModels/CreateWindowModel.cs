using HostingLib.Data.Entities;

namespace Client {
    public class CreateWindowModel : BindableBase {
        private string folderName = string.Empty;
        public string FolderName {
            get => folderName;
            set => SetProperty(ref folderName, value);
        }

        private bool isPublic = false;
        public bool IsPublic {
            get => isPublic;
            set => SetProperty(ref isPublic, value);
        }

        public File? ParentFolder { get; set; }
        public User User { get; set; }

        public CreateWindowModel(File? parentFolder, User user) {
            ParentFolder = parentFolder;
            User = user;
        }
    }
}
