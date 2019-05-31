using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversalFileToPrinter.Models
{
    public class FilePrintRequest
    {
        public string action { get; set; }
        public string pdfUrl { get; set; }
        public string printer { get; set; }
    }
}
