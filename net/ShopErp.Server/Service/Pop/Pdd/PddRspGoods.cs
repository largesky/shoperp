using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pdd
{
    public class PddRspGoods : PddRspBase
    {
        public int total_count;
        public List<PddRspGoodsItem> goods_list;
    }
}
