using Client.Services;
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
        public readonly UserSingleton user_singleton;

        public CreateWindowModel(File? parentFolder) {
            user_singleton = UserSingleton.GetInstance();
            ParentFolder = parentFolder;
        }
    }
}
