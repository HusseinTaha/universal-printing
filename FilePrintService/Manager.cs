using Logs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

namespace FilePrintService
{
    public class Manager
    {
        private static WebSocketHelper socketHelper;

        public static void Start()
        {
            try
            {
                Logger.logFileName = Path.Combine(Manager.GetTempFolder(), "print-file-to-printer-service.log");
            }
            catch (Exception ex)
            {
            }
            Logger.Log("Manager.Start");

            if (!Checklisence())
            {
                return;
            }

            string port = ConfigurationManager.AppSettings["PORT"];
            if (!string.IsNullOrEmpty(port))
            {
                try
                {
                    int _port = Convert.ToInt32(port);
                    Logger.Log("Manager.Start on Port: "+port);
                    socketHelper = new WebSocketHelper(_port);
                    socketHelper.Setup();
                    socketHelper.StartServer();
                }
                catch (Exception ex)
                {
                    Logger.Error("Manager.Start => Error in starting server, " + port, ex);
                }
            }
            else
            {
                Logger.Log("Manager.Start => Port not found");
            }
        }

        public static void Stop()
        {
            try
            {
                Logger.Log("Manager.Stop");
                if (socketHelper != null)
                {
                    socketHelper.StopServer();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Manager.Stop", ex);
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
                    ServiceController sc = new ServiceController("PrintService");
                    sc.Stop();
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

        public static string GetTempFolder()
        {
            return Environment.GetEnvironmentVariable("Temp");
        }

        public static void Print(string printerName, string htmlData)
        {
            try
            {
                Logger.Log("Data received -> " + printerName + " ");
                string _localFileName = Path.Combine(Manager.GetTempFolder(), Guid.NewGuid().ToString() + ".html");
                Logger.Log("writing to :" + _localFileName);
                if (!string.IsNullOrEmpty(htmlData))
                {
                    string d = Uri.UnescapeDataString(htmlData);
                    File.WriteAllText(_localFileName, d);
                    PrintHtmlPages(printerName, _localFileName);
                }
                else
                {
                    Logger.Log("String data empty:");
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static bool PrintHtmlPages(string printer, string url)
        {
            try
            {
                if (ConfigurationManager.AppSettings["BrowserPrint"] == "true")
                {
                    var infoBrowserConsole = new ProcessStartInfo();
                    infoBrowserConsole.Arguments = " \"" + url + "\"   \"" + printer + "\"";
                    //info.Arguments = "printername=\"" + printer + "\"   url=\"" + url + "\"";
                    var pathToBrowserExe = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    infoBrowserConsole.FileName = Path.Combine(pathToBrowserExe, "BrowserPrint.exe");
                    using (var p = Process.Start(infoBrowserConsole))
                    {
                        // Wait until it is finished
                        while (!p.HasExited)
                        {
                            System.Threading.Thread.Sleep(10);
                        }
                        // Return the exit code
                        return p.ExitCode == 0;
                    }

                    return true;
                }
                // Spawn the code to print the packing slips
                var info = new ProcessStartInfo();
                info.Arguments = ConfigurationManager.AppSettings["MARGINS"] + " -p \"" + printer + "\" \"" + url + "\"";
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
            catch(Exception ex)
            {
                Logger.Error("Manager.PrintHtmlPages =>", ex);
                return false;
            }
        }


    }
}