using Logs;
using RawPrint;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniversalFileToPrinter.Helpers
{
    public abstract class BasePrinting
    {
        public bool useRaw = false;

        public  bool Print(string printer, string url, WebSocketSession session, FilePrintHelper filePrint)
        {
            try
            {
                // Create an instance of the Printer
                IPrinter printerRaw = new Printer();

                // Print the file
                printerRaw.PrintRawFile(printer, url, Path.GetFileName(url));

                return true;
            }
            catch (Exception ex)
            {
                filePrint.SendErrorMsg(session, "Print", "Print raw error", ex);
                Logger.Error("RAWPrinting.print =>", ex);
                return false;
            }
        }
    }
}
