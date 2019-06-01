using System;
using System.Collections.Generic;
using System.IO; 
using System.Text;  

namespace Logs
{
    public class Logger
    {
        public static string logFileName;

        public static void Log(string msg)
        {
            string LogFileName = logFileName;
            try
            {
                if (!File.Exists(logFileName))
                {
                    File.WriteAllText(logFileName, "Created file" + Environment.NewLine);
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(msg);
                
                File.AppendAllText(LogFileName, "Debug: " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss.fff") + ": " + sb.ToString() + Environment.NewLine);
                sb = null;
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }
        }

        public static void Error(string msg, Exception ex)
        {
            string LogFileName = logFileName;
            try
            {
                if (!File.Exists(logFileName))
                    File.WriteAllText(logFileName, "Created file" + Environment.NewLine);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(msg);
                sb.AppendLine("MessageError: " + ex.Message);
                sb.AppendLine("InnerException: " + ex.InnerException);
                sb.AppendLine("StackTrace: " + ex.StackTrace);

                File.AppendAllText(LogFileName, "Error: " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss.fff") + ": " + sb.ToString() + Environment.NewLine);
                sb = null;
            }
            catch (Exception e)
            {

            }
        }


    }
}
