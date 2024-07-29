using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Data.Entities
{
    public class User_Files
    {
        public int Id { get; set; }
        [Required]
        public int User_id { get; set; }
        public User User { get; set; }
        [Required]
        public int File_id { get; set; }
        public File File { get; set; }

        public User_Files
            (int user_id, int file_id)
        {
            User_id = user_id;
            File_id = file_id;
        }
    }
}
