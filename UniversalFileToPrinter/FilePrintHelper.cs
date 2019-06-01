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
using SuperWebSocket;
using Newtonsoft.Json;

namespace UniversalFileToPrinter
{
    public class FilePrintHelper
    {
        private int _timeout = 0;
        private int _port;
        private WebSocketHelper myServer;

            #region public methods

        public FilePrintHelper(string argstr)
        {
            try
            {
                argstr = argstr.Replace("ufileprintmagnet:", "");

                var uri = new Uri("http://domain.test/Default.aspx?" + argstr);
                var query = HttpUtility.ParseQueryString(uri.Query);
                Logger.Log("Args:" + argstr);

                var port = query.Get("p");
                var timeout = query.Get("timeout");

                if (!string.IsNullOrEmpty(port))
                {
                    _port = Convert.ToInt32(port);
                }

                if (!string.IsNullOrEmpty(timeout))
                {
                    _timeout = Convert.ToInt32(timeout);
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
                Logger.Log("Port: " + _port + ", Timeout: " + _timeout);
                if (_port != default(int))
                {
                    Logger.Log("Starting server");
                    myServer = new WebSocketHelper(_port, this);
                    myServer.Setup();
                    myServer.StartServer();
                }

                if (_timeout != 0)
                {
                    System.Timers.Timer _timer = new System.Timers.Timer(_timeout);
                    _timer.Elapsed += (s, ee) => { StopAndClose(); };
                    _timer.AutoReset = false;
                    _timer.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Manage", ex);
            }
        }

        public void Print(string type, string extension, string printerName, string fileUrl, byte[] data, object dataInfo, WebSocketSession session)
        {
           try{
               bool printerPlugged = checkPrinter(ref printerName);
                if (!printerPlugged)
                {
                    string msg = "No " + printerName + " online printer found!";
                    SendErrorMsg(session, "Print", msg, null);
                    Logs.Logger.Log(msg);
                    return;
                }

                var _localFileName = Path.Combine(System.IO.Path.GetTempPath(), 
                    Guid.NewGuid().ToString() + (extension.Contains(".")?"":".") + extension);

                if (!string.IsNullOrEmpty(fileUrl))
                {
                    Logger.Log("Downloading:" + fileUrl + "    into    " + _localFileName);
                    Logger.Log("Printer:" + printerName);
                    using (WebClient web = new WebClient())
                    {
                        web.UseDefaultCredentials = true;
                        web.DownloadFile(fileUrl, _localFileName);
                    }
                }
                else if (data != null && data.Length != 0)
                {
                    File.WriteAllBytes(_localFileName, data);
                }

                switch (type)
                {
                    case "PDF":
                        Helpers.CastHelper.CastTo<Helpers.PDFPrinting>(dataInfo).Print(printerName, _localFileName, session, this);
                        break;
                    case "WORD":
                        Helpers.CastHelper.CastTo<Helpers.WORDPrinting>(dataInfo).Print(printerName, _localFileName, session, this);
                        break;
                    case "HTML":
                        Helpers.CastHelper.CastTo<Helpers.HTMLPrinting>(dataInfo).Print(printerName, _localFileName, session, this);
                        break;
                    case "IMAGE":
                        Helpers.CastHelper.CastTo<Helpers.IMAGEPrinting>(dataInfo).Print(printerName, _localFileName, session, this);
                        break;
                    default:
                        Helpers.CastHelper.CastTo<Helpers.RAWPrinting>(dataInfo).Print(printerName, _localFileName, session, this);
                        break;
                }

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

                 
            }
            catch (Exception ex)
            {
                SendErrorMsg(session, "Print", "Print action error", ex);
                Logger.Error("Print", ex);
            }
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

        public void StopAndClose()
        {
            Logger.Log("Closing server");
            if (myServer != null)
            {
                myServer.StopServer();
            }
            System.Environment.Exit(2);
        }

        public void SendErrorMsg(WebSocketSession session, string action, string message, Exception ex)
        {
            try
            {
                if (session != null)
                {
                    session.Send(JsonConvert.SerializeObject(new Models.FilePrintResponse
                    {
                        action = action,
                        isError = true,
                        message = message,
                        additionalInfo = (ex != null ? ex.ToString() : "")
                    }));
                }
            }
            catch (Exception ex2)
            {
                Logger.Error("SendErrormsg", ex2);
            }
        }

            #endregion

            #region private methods

       

        private bool checkPrinter(ref string printerName)
        {
            bool printerPlugged = false;
            if (String.IsNullOrEmpty(printerName))
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
                    if (printer.IsOnline() && printer["Name"].ToString().ToLower() == (printerName.ToLower()))
                    {
                        printerPlugged = true;
                        printerName = printer["Name"].ToString();
                        break;
                    }
                }
            }
            return printerPlugged;
        }  
            #endregion
    }
}
