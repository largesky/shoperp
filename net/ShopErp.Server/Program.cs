using ShopErp.Server.Service;
using ShopErp.Server.Service.Restful;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                var vs = new ServiceContainer();
                vs.Start();
                while (true)
                {
                    string ret = Console.ReadLine();
                    if (ret == "dump")
                    {
                        ServiceContainer.DumpInfo();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                ServiceBase.Run(new ServiceBase[] { new ShopErpService() });
            }
        }
    }
}
