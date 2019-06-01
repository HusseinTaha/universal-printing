using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversalFileToPrinter.Models
{
    public class FilePrintRequest
    {
        public string action { get; set; }
        public string fileUrl { get; set; }
        public string printer { get; set; }
        public string extension { get; set; }
        public byte[] dataBin { get; set; }
        public object info { get; set; }
        public string type { get; set; }
    }
}
