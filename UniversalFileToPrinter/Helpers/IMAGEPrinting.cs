using Logs;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;

namespace UniversalFileToPrinter.Helpers
{
    public class IMAGEPrinting : BasePrinting
    {
        public float width;
        public float height;

        private string _url;
        private FilePrintHelper _filePrint;
        private WebSocketSession _session;

        public new bool Print(string printer, string url, WebSocketSession session, FilePrintHelper filePrint)
        {
            try
            {
                if (base.useRaw)
                {
                    return base.Print(printer, url, session, filePrint);
                }
                _session = session;
                _filePrint = filePrint;
                _url = url;
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrintPage += new PrintPageEventHandler(OnPrintAll);
                if (!String.IsNullOrEmpty(printer))
                {
                    printDocument.PrinterSettings.PrinterName = printer;
                }
                printDocument.Print();
                return true;
            }
            catch (Exception ex)
            {
                filePrint.SendErrorMsg(session, "Print", "Print image error", ex);
                Logger.Error("IMAGEPrinting.print =>", ex);
            }
            return false;
        }

        private void OnPrintAll(object sender, PrintPageEventArgs e)
        {
            try
            {
                Byte[] bitmapData = File.ReadAllBytes(_url);
                MemoryStream streamBitmap = new MemoryStream(bitmapData);
                if (this.width != default(float) && this.width != 0)
                {
                    Rectangle m = e.MarginBounds;
                    m.Height = Convert.ToInt32(this.height);
                    m.Width = Convert.ToInt32(this.width);
                    m.Location = new Point(0, 0);
                    e.Graphics.DrawImage(new Bitmap((Bitmap)Image.FromStream(streamBitmap)), m);
                }
                else
                {
                    e.Graphics.DrawImage(new Bitmap((Bitmap)Image.FromStream(streamBitmap)), 0, 0);
                }
                
                e.HasMorePages = false;
            }
            catch (Exception ex)
            {
                _filePrint.SendErrorMsg(_session, "Print", "Print all image error", ex);
                Logger.Error("IMAGEPrinting.OnPrintAll", ex);
            }
        }
    }
}
