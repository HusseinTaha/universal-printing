using Logs;
using Microsoft.Win32;
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
    public class PDFPrinting : BasePrinting
    {


        public new bool Print(string printer, string url, WebSocketSession session, FilePrintHelper filePrint)
        {
            try
            {
                if (base.useRaw)
                {
                    return base.Print(printer, url, session, filePrint);
                }
                Process.Start(
                    Registry.LocalMachine.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion" +
                        @"\App Paths\AcroRd32.exe").GetValue("").ToString(),
                    string.Format("/h /t \"{0}\" \"{1}\"", url, printer));
                return true;
            }
            catch (Exception ex)
            {
                filePrint.SendErrorMsg(session, "Print", "Print pdf error", ex);
                Logger.Error("PDFPrinting.print =>", ex);
                return false;
            }
        }

    }
}
