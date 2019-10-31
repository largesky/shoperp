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
            string add = "重庆市县武隆县御源大湖区";

            //string[] adds = AddressService.Parse5Address(add, Domain.PopType.PINGDUODUO, Domain.PopType.PINGDUODUO);

           // Console.WriteLine(add + "  :" + string.Join(" ", adds));


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
