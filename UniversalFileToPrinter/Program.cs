using Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace UniversalFileToPrinter
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
            if (!FilePrintHelper.Checklisence())
            {
                return;
            }

            if (args != null && args.Length > 0)
            {
                FilePrintHelper fileprint = new FilePrintHelper(args[0]);
                fileprint.Manage();
                //Console.ReadKey();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainPage());
            }

        }
    }
}
