using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom
{
    internal class DocError
    {
        public string errorDescription { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public bool IsSolved { get; set; }
    }
}
