using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Log
{
    public class Logger
    {
        private static readonly string FILE_PREFIX = "log";

        public static void Log(string title, Exception ex)
        {
            lock (FILE_PREFIX)
            {
                string fileName = System.IO.Path.Combine(Utils.EnvironmentDirHelper.DIR_LOG, FILE_PREFIX + DateTime.Now.ToString("_yyyy_MM_dd") + ".txt");
                System.IO.File.AppendAllText(fileName, title, Encoding.Default);
                System.IO.File.AppendAllText(fileName, ex.Message, Encoding.Default);
                System.IO.File.AppendAllText(fileName, ex.StackTrace, Encoding.Default);
            }
        }

        public static void Log(params string[] msgs)
        {
            lock (FILE_PREFIX)
            {
                string fileName = System.IO.Path.Combine(Utils.EnvironmentDirHelper.DIR_LOG, FILE_PREFIX + DateTime.Now.ToString("_yyyy_MM_dd") + ".txt");
                foreach (var s in msgs)
                {
                    System.IO.File.AppendAllText(fileName, s, Encoding.Default);
                }
            }
        }
    }
}
