using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Order
{
    class TaobaoQueryOrderDetailResponseOrder
    {
        public TaobaoQueryOrderDetailResponseOrderPrice[] totalPrice;

        public TaobaoQueryOrderDetailResponseOrderOrderGoods[] subOrders;

        public TaobaoQueryOrderDetailResponseOrderInfo orderInfo;

        public TaobaoQueryOrderDetailResponseOrderPayInfo payInfo;

        public TaobaoQueryOrderDetailResponseOrderStatusInfo statusInfo;
    }
}
