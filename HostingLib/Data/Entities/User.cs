using System;
using System.Collections.Generic;
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
        public byte[] EncryptionKey { get; set; }
        [Required]
        [Column(TypeName = "varbinary(16)")]
        public byte[] Iv { get; set; }
        public ICollection<File> Files { get; set; } = new List<File>();

        public User(string email, string password, bool permission, byte[] encryptionKey, byte[] iv)
        {
            Email = email;
            Password = password;
            Permission = permission;
            EncryptionKey = encryptionKey;
            Iv = iv;
        }
    }
}
