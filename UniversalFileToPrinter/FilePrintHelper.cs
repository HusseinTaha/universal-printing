using UniversalFileToPrinter;
using Logs;
using Microsoft.Win32;
using UniversalFileToPrinter;
using RawPrint;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Management;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace UniversalFileToPrinter
{
    public class FilePrintHelper
    {
        private string _printerName;
        private string _localFileName = "";
        public string _ajaxed = "0";
        private int _port;
        private WebSocketHelper myServer;

        public FilePrintHelper(string argstr)
        {
            try
            {
                argstr = argstr.Replace("sfileprintmagnet:", "");

                var uri = new Uri("http://domain.test/Default.aspx?" + argstr);
                var query = HttpUtility.ParseQueryString(uri.Query);
                Logger.Log("Args:" + argstr);

                var port = query.Get("p");

                if (!string.IsNullOrEmpty(port))
                {
                    _port = Convert.ToInt32(port);
                }

            }
            catch (Exception ex)
            {
                Logger.Error("FilePrintHelper::", ex);
            }
        }

        public static bool Checklisence()
        {
            try
            {
                DateTime myDate = DateTime.ParseExact("2050-11-21 05:00:00,531", "yyyy-MM-dd HH:mm:ss,fff",
                                      System.Globalization.CultureInfo.InvariantCulture);
                if (myDate < DateTime.Now)
                {
                    Logger.Error("License:", new Exception("License is expired please contact the administrator"));
                    System.Environment.Exit(2);
                    return false; ;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Manager.Start => Checking lisence", ex);
                return false;
            }
        }

        public void Manage()
        {
            try
            {

                if (_port != default(int))
                {
                    Logger.Log("Starting server" );
                    myServer = new WebSocketHelper(_port, this);
                    myServer.Setup();
                    myServer.StartServer();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Manage", ex);
            }
        }

        public void StopAndClose()
        {
            Logger.Log("Closing server" );
            if (myServer != null)
            {
                myServer.StopServer();
            }
            System.Environment.Exit(2);
        }

        public List<Models.PrinterCls> ListPrinters()
        {
            List<Models.PrinterCls> printers = new List<Models.PrinterCls>();
            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            foreach (var printer in printerQuery.Get())
            {
                try
                {
                    var name = printer.GetPropertyValue("Name");
                    var status = printer.GetPropertyValue("Status");
                    var isDefault = printer.GetPropertyValue("Default");
                    var isNetworkPrinter = printer.GetPropertyValue("Network");
                    printers.Add(new Models.PrinterCls()
                    {
                        name = name.ToString(),
                        status = status.ToString(),
                        isOnline = printer.IsOnline(),
                        isDefault = isDefault.ToString(),
                        isNetwrokPrinter = isNetworkPrinter.ToString()
                    });
                    Console.WriteLine("{0} (Status: {1}, Default: {2}, Network: {3}",
                                name, status, isDefault, isNetworkPrinter);
                }
                catch (Exception ex)
                {
                    Logger.Error("ListPrinters", ex);
                }
            }
            return printers;
        }

        public void Print(string printerName, string pdfUrl)
        {
            this._printerName = printerName;
            try
            {
                bool printerPlugged = false;
                if (String.IsNullOrEmpty(this._printerName))
                {
                    foreach (ManagementObject printer in PrinterHelper.GetDefaultPrinters())
                    {
                        if (printer.IsOnline())
                        {
                            printerPlugged = true;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (ManagementObject printer in PrinterHelper.GetPrinters())
                    {
                        if (printer.IsOnline() && printer["Name"].ToString().ToLower() == (this._printerName.ToLower()))
                        {
                            printerPlugged = true;
                            this._printerName = printer["Name"].ToString();
                            break;
                        }
                    }
                }
                if (!printerPlugged)
                {
                    Logs.Logger.Log("No " + _printerName + " online printer found!");
                    return;
                }

                this._localFileName = Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");

                Logger.Log("Downloading:" + pdfUrl + "    into    " + _localFileName);
                Logger.Log("Printer:" + _printerName) ;
                using (WebClient web = new WebClient())
                {
                    web.UseDefaultCredentials = true;
                    web.DownloadFile(pdfUrl, this._localFileName);
                }


                //PrinterSettings printerSett = new PrinterSettings();

                //printerSett.PrinterName = _printerName;
                //printerSett.PrintFileName = Path.GetFileName(_localFileName);

                //PrintDocument PrintDoc = new PrintDocument();
                //PrintDoc.PrinterSettings = printerSett;
                //PrintDoc.DocumentName = this._localFileName;// @"..\Resources\" + name;
                //PrintDoc.PrinterSettings.PrinterName = _printerName;

                //PrintDoc.Print();

            //    string processFilename = Microsoft.Win32.Registry.LocalMachine
            //.OpenSubKey("Software")
            //.OpenSubKey("Microsoft")
            //.OpenSubKey("Windows")
            //.OpenSubKey("CurrentVersion")
            //.OpenSubKey("App Paths")
            //.OpenSubKey("AcroRd32.exe")
            //.GetValue(String.Empty).ToString();

            //    ProcessStartInfo info = new ProcessStartInfo();
            //    info.Verb = "print";
            //    info.FileName = processFilename;
            //    info.Arguments = string.Format("/h /t \"{0}\" \"{1}\"", this._localFileName, this._printerName);
            //    info.CreateNoWindow = true;
            //    info.WindowStyle = ProcessWindowStyle.Hidden;
            //    //(It won't be hidden anyway... thanks Adobe!)
            //    info.UseShellExecute = false;

            //    Process p = Process.Start(info);
            //    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                

            //    int counter = 0;
            //    while (!p.HasExited)
            //    {
            //        System.Threading.Thread.Sleep(1000);
            //        counter += 1;
            //        if (counter == 5) break;
            //    }
            //    if (!p.HasExited)
            //    {
            //        p.CloseMainWindow();
            //        p.Kill();
            //    }

                try
                {
                    Process.Start(
                       Registry.LocalMachine.OpenSubKey(
                            @"SOFTWARE\Microsoft\Windows\CurrentVersion" +
                            @"\App Paths\AcroRd32.exe").GetValue("").ToString(),
                       string.Format("/h /t \"{0}\" \"{1}\"", this._localFileName, this._printerName));
                }
                catch { } 
                //IPrinter printDocument = new Printer();

                //Printer.PrintFile(this._printerName, this._localFileName);

                 //Print the file
                //printDocument.PrintRawFile(this._printerName, this._localFileName, Path.GetFileName(this._localFileName));

                //PrintDocument printDocument = new PrintDocument();
                //printDocument.PrintPage += new PrintPageEventHandler(OnPrintAll);
                //if (!String.IsNullOrEmpty(this._printerName))
                //{
                //    printDocument.PrinterSettings.PrinterName = this._printerName;
                //}
                //printDocument.Print();
            }
            catch (Exception ex)
            {
                Logger.Error("Print", ex);
            }
        }

        public bool PrintHtmlPages(string printer, string url)
        {
            try
            {
                // Spawn the code to print the packing slips
                var info = new ProcessStartInfo();
                info.Arguments = "-p \"" + printer + "\" \"" + url + "\"";
                //info.Arguments = "printername=\"" + printer + "\"   url=\"" + url + "\"";
                var pathToExe = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                info.FileName = Path.Combine(pathToExe, "PrintHtml.exe");
                using (var p = Process.Start(info))
                {
                    // Wait until it is finished
                    while (!p.HasExited)
                    {
                        System.Threading.Thread.Sleep(10);
                    }

                    // Return the exit code
                    return p.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private string getExtension(string filename)
        {
            var name = Guid.NewGuid().ToString();
            //if (filename.Contains(".php") || filename.Contains(".asp") || filename.Contains(".aspx"))
            //{
            //    return name + ".html";
            //}
            return name + ".pdf";
        }
    }
}
