using Logs;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UniversalFileToPrinter.Helpers
{
    public class HTMLPrinting : BasePrinting
    {
        public bool useBrowserPrint = false;
        public string margins = "";


        public new bool Print(string printer, string url, WebSocketSession session, FilePrintHelper filePrint)
        {
            try
            {
                if (base.useRaw)
                {
                    return base.Print(printer, url, session, filePrint);
                }
                if (useBrowserPrint)
                {
                    var infoBrowserConsole = new ProcessStartInfo();
                    infoBrowserConsole.Arguments = " \"" + url + "\"   \"" + printer + "\"";
                    //info.Arguments = "printername=\"" + printer + "\"   url=\"" + url + "\"";
                    var pathToBrowserExe = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    infoBrowserConsole.FileName = Path.Combine(pathToBrowserExe, "BrowserPrint.exe");
                    Logger.Log("Starting browser-print:" + infoBrowserConsole.FileName);
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
                info.Arguments = margins + " -p \"" + printer + "\" \"" + url + "\"";
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
            catch (Exception ex)
            {
                filePrint.SendErrorMsg(session, "Print", "Print html error", ex);
                Logger.Error("HTMLPrinting.print =>", ex);
                return false;
            }
        }

    }
}
