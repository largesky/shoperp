using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Order
{
    public class TaobaoQueryOrderListResponseOrder
    {
        public string id;

        public TaobaoQueryOrderListResponseOrderExtra extra;

        public TaobaoQueryOrderListResponseOrderBuyer buyer;

        public TaobaoQueryOrderListResponseOrderPayInfo payInfo;

        public TaobaoQueryOrderListResponseOrderStatusInfo statusInfo;

        public TaobaoQueryOrderListResponseOrderOrderGoods[] subOrders;
    }
}