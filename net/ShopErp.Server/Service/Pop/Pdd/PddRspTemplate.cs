using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pdd
{
    class PddRspTemplate : PddRspBase
    {
        public int total_count;

        public PddRspTemplateItem[] logistics_template_list;
    }
}
