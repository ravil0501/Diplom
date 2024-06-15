using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom.Models
{
    public class FileImport
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public DateTime CreationDate { get; set; }
        public string UserLogin { get; set; }
        public int? GroupNumber { get; set; }
        public byte[] FileData { get; set; }
    }
}
