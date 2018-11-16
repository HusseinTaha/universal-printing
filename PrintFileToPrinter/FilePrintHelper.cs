using Logs;
using Microsoft.Win32;
using PrintingFunctionality;
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
using System.Windows.Forms;

namespace PrintFileToPrinter
{
    public class FilePrintHelper
    {
        private string _printerName;
        private string _localFileName = "";
        private string _url = "";
        private string _data = "";
        public string _ajaxed = "0";

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
                            case "data":
                                this._data = keyval[1];
                                break;
                            case "printerName":
                                this._printerName = keyval[1];
                                break;
                            case "ajaxed":
                                this._ajaxed = keyval[1];
                                break;
                        }
                    }
                }
                //string filname = getExtension(_url);
                this._localFileName = Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".html");
                Logger.Log("writing to :" + _localFileName);
                if (!string.IsNullOrEmpty(_data))
                {
                    string d = Uri.UnescapeDataString(_data);
                    File.WriteAllText(_localFileName, d);
                    this._url = _localFileName;
                }
                else if (_ajaxed == "1")
                {
                    BrowserWrapper browser = new BrowserWrapper();
                    Logger.Log("Loading to :" + _url);
                    browser.NavigateAndWait(this._url);
                    Logger.Log("Done loading to :" + _url);
                    Thread.Sleep(1500);
                    HtmlDocument doc = browser.Document;
                    File.WriteAllText(_localFileName, doc.Body.OuterHtml.ToString());
                    this._url = _localFileName;
                    browser.Dispose();
                }
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
                //Logger.Log("Downloading:" + _url + "    into    " + _localFileName);
                //using (WebClient web = new WebClient())
                //{
                //    web.DownloadFile(this._url, this._localFileName);
                //}

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

                PrintHtmlPages(this._printerName, this._url);

                //try
                //{
                //    Process.Start(
                //       Registry.LocalMachine.OpenSubKey(
                //            @"SOFTWARE\Microsoft\Windows\CurrentVersion" +
                //            @"\App Paths\AcroRd32.exe").GetValue("").ToString(),
                //       string.Format("/h /t \"{0}\" \"{1}\"", this._localFileName, this._printerName)); 
                //}
                //catch { } 
                //IPrinter printDocument = new Printer();

                //Printer.PrintFile(this._printerName, this._localFileName);

                // Print the file
                //printDocument.PrintRawFile(this._printerName, this._localFileName);

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
