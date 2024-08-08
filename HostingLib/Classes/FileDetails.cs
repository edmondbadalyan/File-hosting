using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Classes
{
    public class FileDetails
    {
        public string Name { get; set; }
        public long Length { get; set; }
        public string Extension { get; set; }
        public DateTime CreationTime { get; set; }

        public FileDetails(string name, long length, string extension, DateTime creationTime)
        {
            Name = name;
            Length = length;
            Extension = extension;
            CreationTime = creationTime;
        }

        public FileDetails()
        {
        }
    }
}
