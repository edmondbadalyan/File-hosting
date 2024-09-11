using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public bool Permission { get; set; }
        [Required]
        public bool IsPublic { get; set; }
        [Required]
        public TimeSpan AutoFileDeletionTime { get; set;}
        public ICollection<File> Files { get; set; } = new List<File>();

        public User(string email, string password, bool permission, bool isPublic, TimeSpan? auto_file_deletion_time)
        { 
            Email = email;
            Password = password;
            Permission = permission;
            IsPublic = isPublic;
            AutoFileDeletionTime = auto_file_deletion_time ?? TimeSpan.FromDays(30);
        }

        public User() {}
    }
}
