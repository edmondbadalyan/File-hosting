using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Data.Entities
{
    public class File
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Path { get; set; }
        [Required]
        public double Size { get; set; }
        [Required]
        public DateTime ChangeDate { get; set; }
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        [Required]
        public bool IsDirectory { get; set; }
        [Required]
        public int ParentId { get; set; }
        public File Parent { get; set; }
        public ICollection<File> Children { get; set; }

        public File(string name, string path, double size, DateTime changeDate, int userId, int parentId, bool isDirectory = false)
        {
            Name = name;
            Path = path;
            Size = size;
            ChangeDate = changeDate;
            UserId = userId;
            ParentId = parentId;
            IsDirectory = isDirectory;
        }
    }
}
