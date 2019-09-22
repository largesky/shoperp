using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Order
{
    public class TaobaoQueryOrderDetailResponseOverStatus
    {
        public TaobaoQueryOrderDetailResponseOverStatusStatus status;

        public TaobaoQueryOrderDetailResponseOverStatusOperate[] operate;

        public TaobaoQueryOrderDetailResponseOverStatusPrompt[] prompt;
    }
}