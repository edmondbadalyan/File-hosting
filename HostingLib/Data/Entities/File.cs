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
        public int UserId { get; set; }
        public User User { get; set; }

        public File(string name, string path, int userId)
        {
            Name = name;
            Path = path;
            UserId = userId;
        }
    }
}
