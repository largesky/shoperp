using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Views.Orders.Taobao
{
    public class TaobaoQueryOrdersResponseOrder
    {
        public string id;

        public TaobaoQueryOrdersResponseOrderExtra extra;

        public TaobaoQueryOrdersResponseOrderBuyer buyer;

        public TaobaoQueryOrdersResponseOrderOrderInfo orderInfo;

        public TaobaoQueryOrdersResponseOrderPayInfo payInfo;

        public TaobaoQueryOrdersResponseOrderStatusInfo statusInfo;

        public TaobaoQueryOrdersResponseOrderOrderGoods[] subOrders;
    }
}