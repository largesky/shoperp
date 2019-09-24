using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Order
{
    class TaobaoQueryOrderDetailResponse
    {
        public string buyMessage;

        public TaobaoQueryOrderDetailResponseOrder mainOrder;

        public TaobaoQueryOrderDetailResponseTab[] tabs;

        public TaobaoQueryOrderDetailResponseOperationsGuide[] operationsGuide;
    }
}
