using System;
using System.Collections.Generic; 
using System.Text;
using System.Reflection;
using System.Threading;
using System.Management;
using Logs;
using PrintFileToPrinter;

namespace FilePrintService
{
    public class IEHTMLPrinter
    {
        private string originalDefaultPrinterName;
         

        public void Print(string htmlFilename, string printerName)
        {
            // Preserve default printer name
            originalDefaultPrinterName = GetDefaultPrinterName();
            // set new default printer
            SetDefaultPrinter(printerName);
            // print to printer
            Print(htmlFilename);
        }

        public void Print(string htmlFilename)
        {
            BrowserWrapper browser = new BrowserWrapper();
            Logger.Log("Loading to :" + htmlFilename);
            browser.Navigate(htmlFilename);
            Thread.Sleep(2000);
            Logger.Log("Trying to print");
            browser.Print();
            Logger.Log("Printing Done");
            Thread.Sleep(1000);
            //HtmlDocument doc = browser.Document;

            // reset to original default printer if needed
            if (GetDefaultPrinterName() != originalDefaultPrinterName)
            {
                SetDefaultPrinter(originalDefaultPrinterName);
            }
        }

        public static string GetDefaultPrinterName()
        {
            var query = new ObjectQuery("SELECT * FROM Win32_Printer");
            var searcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject mo in searcher.Get())
            {
                if (((bool?)mo["Default"]) ?? false)
                {
                    return mo["Name"] as string;
                }
            }

            return null;
        }

        public static bool SetDefaultPrinter(string defaultPrinter)
        {
            using (ManagementObjectSearcher objectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer"))
            {
                using (ManagementObjectCollection objectCollection = objectSearcher.Get())
                {
                    foreach (ManagementObject mo in objectCollection)
                    {
                        if (string.Compare(mo["Name"].ToString(), defaultPrinter, true) == 0)
                        {
                            Logger.Log("Set detault printer =>" + defaultPrinter);
                            mo.InvokeMethod("SetDefaultPrinter", null, null);
                            return true;
                        }
                    }
                }
            }
            return true;
        }
    }

}
