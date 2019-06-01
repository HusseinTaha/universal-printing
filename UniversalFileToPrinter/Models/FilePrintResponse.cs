using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversalFileToPrinter.Models
{
    public class FilePrintResponse
    {
        public string action { get; set; }
        public bool isError { get; set; }
        public string message { get; set; }
        public string additionalInfo { get; set; }
    }
}
