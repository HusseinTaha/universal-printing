using FilePrintService;
using Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrowserPrint
{
    class Program
    {
         [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Logger.logFileName = Path.Combine(Environment.GetEnvironmentVariable("Temp"), "print-file-to-printer-service.log");
            }
            catch (Exception ex)
            {
            }

            if (args == null)
            {
                Console.WriteLine("args is null");
            }
            else
            {
                try
                {
                    string url = args[0];
                    string printer = args[1];

                    Logger.Log("Url:" + url);
                    Logger.Log("printer:" + printer);
                    IEHTMLPrinter printObj = new IEHTMLPrinter();
                    printObj.Print(url, printer);
                }
                catch (Exception ex)
                {
                    Logger.Error("Main.BrowserPrint =>", ex);
                }
                
            }
        }
    }
}
