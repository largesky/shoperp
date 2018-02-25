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

        static void Test(string add)
        {
            var ors = ServiceContainer.GetService<OrderService>().GetByAll("", "", "", "", add, 0, DateTime.MinValue, DateTime.MinValue, "", "", Domain.OrderState.NONE, Domain.PopPayType.None, "", "", null, -1, "", 0, Domain.OrderCreateType.NONE, Domain.OrderType.NONE, 0, 100);
            foreach (var o in ors.Datas)
            {
                var s = AddressService.ParseProvince(o.ReceiverAddress);
                var c = AddressService.ParseCity(o.ReceiverAddress);
                var r = AddressService.ParseRegion(o.ReceiverAddress);
                string msg = string.Format("{0},S:{1},C:{2},R:{3}", o.ReceiverAddress, s == null ? "NULL" : s.Name, c == null ? "NULL" : c.Name, r == null ? "NULL" : r.Name);
                Debug.WriteLine(msg);
            }
        }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                var vs = new ServiceContainer();
                vs.Start();
                //string[] ss = new string[] { "新疆", "西藏", "内蒙古", "广西" };
                //foreach (var v in ss)
                //    Test(v);

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
