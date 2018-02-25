using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Utils
{
    public class FileUtil
    {
        public static void EnsureExits(System.IO.FileInfo fileInfo)
        {
            if (System.IO.Directory.Exists(fileInfo.DirectoryName))
            {
                return;
            }

            List<string> ss = new List<string>();
            System.IO.DirectoryInfo di = fileInfo.Directory;
            while (di.Exists == false)
            {
                ss.Add(di.FullName);
                di = di.Parent;
            }

            foreach (var dir in ss)
            {
                System.IO.Directory.CreateDirectory(dir);
            }
        }
    }
}
