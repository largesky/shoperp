using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pingduoduo
{
    public class PingduoduoRspGoodsItem
    {
        public string goods_id;
        public string goods_name;
        public string image_url;
        public string is_more_sku;
        public int goods_quantity;
        public string is_onsale;

        public PingduoduoRspGoodsItemSku[] sku_list;
    }
}
