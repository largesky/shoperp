using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Views.Orders.Taobao
{
    public class TaobaoQueryOrdersResponse
    {
        public string error;

        public TaobaoQueryOrdersResponsePage page;

        public TaobaoQueryOrdersResponseOrder[] mainOrders;
    }
}