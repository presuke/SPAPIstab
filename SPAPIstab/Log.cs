using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SPAPIstab
{
    class Log
    {
        internal const String LOG_DIR = @"log";

        public static void outputLog(String kind, String msg)
        {
            try
            {
                if (!Directory.Exists(LOG_DIR))
                {
                    Directory.CreateDirectory(LOG_DIR);
                }

                StreamWriter objLogger = new StreamWriter(LOG_DIR + @"\" + @"log_" + kind + "_" + DateTime.Now.ToString("yyyyMMdd") + @".log", true);

                objLogger.WriteLine(DateTime.Now + "\t" + msg);
                objLogger.Flush();
                objLogger.Close();
                objLogger = null;

            }
            catch (Exception ex)
            {

            }
        }

        public static void outputError(Exception exception)
        {
            try
            {
                if (exception.GetType().Name == "OutOfMemoryException")
                {
                    GC.Collect();
                }

                if (!Directory.Exists(LOG_DIR))
                {
                    Directory.CreateDirectory(LOG_DIR);
                }

                StreamWriter objLogger = new StreamWriter(LOG_DIR + @"\" + @"log_err_" + DateTime.Now.ToString("yyyyMMdd") + @".log", true);

                objLogger.WriteLine(DateTime.Now + "\t" + exception);
                objLogger.Flush();
                objLogger.Close();
                objLogger = null;

            }
            catch (Exception ex)
            {

            }
        }

        public static void deleteLog(String path, String pattern, double limitHour)
        {
            try
            {
                foreach (String strFileName in Directory.GetFiles(path, pattern))
                {
                    if (File.GetLastWriteTime(strFileName) <= DateTime.Now.AddHours(-1 * limitHour))
                        File.Delete(strFileName);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
