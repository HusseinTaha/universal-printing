﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using Logs;

namespace PrintingFunctionality
{
    public static class PrinterHelper
    {
        public static bool IsOnline(this ManagementBaseObject printer)
        {
            try
            {
                var status = printer["PrinterStatus"];
                var workOffline = (bool)printer["WorkOffline"];
                if (workOffline) return false;

                int statusAsInteger = Int32.Parse(status.ToString());
                switch (statusAsInteger)
                {
                    case 3: //Idle
                    case 4: //Printing
                    case 5: //Warming up
                    case 6: //Stopped printing
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("IsOnline", ex);
            }
            return false;
        }

        public static ManagementObjectCollection GetDefaultPrinters()
        {
            var printerSearcher =
              new ManagementObjectSearcher(
                "SELECT * FROM Win32_Printer where Default = true"
              );
            return printerSearcher.Get();
        }

        public static ManagementObjectCollection GetPrinters()
        {
            var printerSearcher =
              new ManagementObjectSearcher(
                "SELECT * FROM Win32_Printer"
              );
            return printerSearcher.Get();
        }
    }
}
