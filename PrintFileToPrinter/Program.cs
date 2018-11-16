using Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace PrintFileToPrinter
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Logger.logFileName = Path.Combine(System.IO.Path.GetTempPath(), "print-file-to-printer.log");
            }
            catch (Exception ex)
            {
            } 
            DateTime myDate = DateTime.ParseExact("2018-11-18 00:00:00,531", "yyyy-MM-dd HH:mm:ss,fff",
                                       System.Globalization.CultureInfo.InvariantCulture);
            if (myDate < DateTime.Now)
            {
                Logger.Error("License:", new Exception("License is expired please contact the administrator"));
                return;
            }

            if (args != null && args.Length > 0)
            {
                FilePrintHelper fileprint = new FilePrintHelper(args[0]);
                fileprint.Manage();
            }

        }
    }
}
