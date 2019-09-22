using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Order
{
    public class TaobaoQueryOrdersResponseOrderOrderGoods
    {
        public TaobaoQueryOrdersResponseOrderOrderGoodsItemInfo itemInfo;

        public TaobaoQueryOrdersResponseOrderOrderGoodsOperation[] operations;

        public int quantity;
    }
}