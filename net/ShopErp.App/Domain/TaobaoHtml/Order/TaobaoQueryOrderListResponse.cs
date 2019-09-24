using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Order
{
    public class TaobaoQueryOrderListResponse
    {
        public string error;

        public TaobaoQueryOrderListResponsePage page;

        public TaobaoQueryOrderListResponseOrder[] mainOrders;
    }
}