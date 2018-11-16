using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilePrintService.Models
{
    public class FilePrintRequest
    {
        public string action { get; set; }
        public string htmlData { get; set; }
        public string printer { get; set; }
    }
}
