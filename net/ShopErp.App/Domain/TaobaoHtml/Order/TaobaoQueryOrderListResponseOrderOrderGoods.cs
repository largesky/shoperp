using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Order
{
    public class TaobaoQueryOrderListResponseOrderOrderGoods
    {
        public TaobaoQueryOrderListResponseOrderOrderGoodsItemInfo itemInfo;

        public TaobaoQueryOrderListResponseOrderOrderGoodsOperation[] operations;

        public int quantity;
    }
}