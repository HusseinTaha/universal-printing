using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversalFileToPrinter.Models
{
    public class PrinterCls
    {
        public string name { get; set; }
        public string status { get; set; }
        public bool isOnline { get; set; }
        public string isDefault { get; set; }
        public string isNetwrokPrinter { get; set; }
    }
}
