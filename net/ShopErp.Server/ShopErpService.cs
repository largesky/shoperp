using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using ShopErp.Server.Log;
using ShopErp.Server.Service.Restful;

namespace ShopErp.Server
{
    public partial class ShopErpService : ServiceBase
    {

        ServiceContainer sc = new ServiceContainer();

        public ShopErpService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                sc.Start();
            }
            catch (Exception e)
            {
                Logger.Log("启动服务失败", e);
                throw e;
            }
        }

        protected override void OnStop()
        {
            sc.Stop();
        }
    }
}
