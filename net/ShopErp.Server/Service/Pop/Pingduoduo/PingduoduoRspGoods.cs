using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pingduoduo
{
    public class PingduoduoRspGoods : PingduoduoRspBase
    {
        public int total_count;
        public List<PingduoduoRspGoodsItem> goods_list;
    }
}
