using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pingduoduo
{
    class PingduoduoRspTemplate : PingduoduoRspBase
    {
        public int total_count;

        public PingduoduoRspTemplateItem[] logistics_template_list;
    }
}
