using Logs;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace UniversalFileToPrinter.Helpers
{
    public class WORDPrinting : BasePrinting
    {
        private const int _exceptionLimit = 4;
        private Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application { Visible = false };

        private FilePrintHelper _filePrint;
        private WebSocketSession _session;

        //private Microsoft.Office.Interop.Word.Document doc;
        // where did you get this file name?
         

        //public bool Print(string printer, string url)
        //{
        //    try
        //    {
        //        doc = word.Documents.Open(url, ReadOnly: true, Visible: false);

        //        object copies = "1";
        //        object pages = "";
        //        object range = Microsoft.Office.Interop.Word.WdPrintOutRange.wdPrintAllDocument;
        //        object items = Microsoft.Office.Interop.Word.WdPrintOutItem.wdPrintDocumentContent;
        //        object pageType = Microsoft.Office.Interop.Word.WdPrintOutPages.wdPrintAllPages;
        //        object oTrue = true;
        //        object oFalse = false;


        //        if (doc != null)
        //        {
        //            doc.Application.ActivePrinter = printer;
        //            doc.PrintOut(ref oTrue, ref oFalse, ref range, ref missing, ref missing, ref missing,
        //                    ref items, ref copies, ref pages, ref pageType, ref oFalse, ref oTrue,
        //                    ref missing, ref oFalse, ref missing, ref missing, ref missing, ref missing);
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("WORDPrinting.print =>", ex);
        //        return false;
        //    }
        //}



        // should be a singleton instance of wrapper for Word
        // the code below assumes this was set beforehand
        // (e.g. from another helper method)
        //private static Microsoft.Office.Interop.Word._Application _app;

        public new bool Print(string printer, string fileName, WebSocketSession session, FilePrintHelper filePrint)
        {
            try
            {
                if (base.useRaw)
                {
                    return base.Print(printer, fileName, session, filePrint);
                }
                _session = session;
                _filePrint = filePrint;
                word.Visible = false;
                // Sometimes Word fails, so needs to be restarted.
                // Sometimes it's not Word's fault.
                // Either way, having this in a retry-loop is more robust.
                for (int retry = 0; retry < _exceptionLimit; retry++)
                {
                    if (TryOncePrintToSpecificPrinter(fileName, printer, retry))
                        break;

                    if (retry == _exceptionLimit - 1) // this was our last chance
                    {
                        // if it didn't have actual exceptions, but was not able to change the printer, we should notify somebody:
                        throw new Exception("Failed to change printer.");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                filePrint.SendErrorMsg(session, "Print", "Print word error", ex);
                Logger.Error("WORDPrinting.print =>", ex);
                return false;
            }
        }

        private bool TryOncePrintToSpecificPrinter(string fileName, string printer, int retry)
        {
            Microsoft.Office.Interop.Word.Document doc = null;

            try
            {
                doc = OpenDocument(fileName);

                if (!SetActivePrinter(doc, printer))
                    return false;

                Print(doc);

                if (doc != null)
                {
                    object saveOptionsObject = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                    object missingValue = Missing.Value;
                    doc.Close(ref saveOptionsObject, ref missingValue, ref missingValue);

                    // Application
                    if (word != null)
                    {
                        word.Quit(saveOptionsObject, missingValue, missingValue);
                    }
                }

                return true; // we did what we wanted to do here
            }
            catch (Exception e)
            {
                if (retry == _exceptionLimit)
                {
                    throw new Exception("Word printing failed.", e);
                }
                _filePrint.SendErrorMsg(_session, "Print", "Print word error", e);
                // restart Word, remembering to keep an appropriate delay between Quit and Start.
                // this should really be handled by wrapper classes
            }
            finally
            {
                if (doc != null)
                {
                    // release your doc (COM) object and do whatever other cleanup you need
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
                    if (word != null)
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(word);
                    doc = null;
                    word = null;
                    GC.Collect();
                }
            }

            return false;
        }

        private void Print(Microsoft.Office.Interop.Word.Document doc)
        {
            // do the actual printing:
            doc.Activate();
            Thread.Sleep(TimeSpan.FromSeconds(1)); // emperical testing found this to be sufficient for our system
            // (a delay may not be required for you if you are printing only one document at a time)
            doc.PrintOut(/* ref objects */);
        }

        private bool SetActivePrinter(Microsoft.Office.Interop.Word.Document doc, string printer)
        {
            string oldPrinter = GetActivePrinter(doc); // save this if you want to preserve the existing "default"

            if (printer == null)
                return false;

            if (oldPrinter != printer)
            {
                // conditionally change the default printer ...
                // we found it inefficient to change the default printer if we don't have to. YMMV.
                doc.Application.ActivePrinter = printer;
                Thread.Sleep(TimeSpan.FromSeconds(5)); // emperical testing found this to be sufficient for our system
                if (GetActivePrinter(doc) != printer)
                {
                    // don't quit-and-restart Word, this one actually isn't Word's fault -- just try again
                    return false;
                }

                // successful printer switch! (as near as anyone can tell)
            }
            return true;
        }

        private Microsoft.Office.Interop.Word.Document OpenDocument(string fileName)
        {
            return word.Documents.Open(fileName, ReadOnly: true, Visible: false);
        }

        private string GetActivePrinter(Microsoft.Office.Interop.Word._Document doc)
        {
            string activePrinter = doc.Application.ActivePrinter;
            int onIndex = activePrinter.LastIndexOf(" on ");
            if (onIndex >= 0)
            {
                activePrinter = activePrinter.Substring(0, onIndex);
            }
            return activePrinter;
        }
    }
}