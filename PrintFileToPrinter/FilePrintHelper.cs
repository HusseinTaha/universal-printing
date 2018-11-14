using Logs;
using PrintingFunctionality;
using RawPrint;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Management;
using System.Net;
using System.Text;

namespace PrintFileToPrinter
{
    public class FilePrintHelper
    {
        private string _printerName;
        private string _localFileName = "";
        private string _url = "";

        public FilePrintHelper(string argstr)
        {
            try
            {
                argstr = argstr.Replace("fileprintmagnet:", "");
                Logger.Log("Args:" + argstr);
                string[] values = argstr.Split(new string[] { "=+=" }, StringSplitOptions.None);
                foreach (var val in values)
                {
                    if (!string.IsNullOrEmpty(val))
                    {
                        string[] keyval = val.Split(new string[] { "_=" }, StringSplitOptions.None);
                        switch (keyval[0])
                        {
                            case "url":
                                this._url = keyval[1];
                                break;
                            case "printerName":
                                this._printerName = keyval[1];
                                break;
                        }
                    }
                }
                string filname = getExtension(_url);
                this._localFileName = Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid() +
                    Path.GetExtension(filname));

            }
            catch (Exception ex)
            {
                Logger.Error("FilePrintHelper::", ex);
            }
        }

        public void Manage()
        {
            try
            {
                Logger.Log("Downloading:" + _url + "    into    " + _localFileName);
                using (WebClient web = new WebClient())
                {
                    web.DownloadFile(this._url, this._localFileName);
                }

                this.Print();
            }
            catch (Exception ex)
            {
                Logger.Error("Manage", ex);
            }
        }

        public void Print()
        {
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
                        if (printer.IsOnline() && printer["Name"].ToString().ToLower().Contains(this._printerName.ToLower()))
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


                IPrinter printDocument = new Printer();

                // Print the file
                printDocument.PrintRawFile(this._printerName, this._localFileName, Path.GetFileName(this._localFileName));

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
