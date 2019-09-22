using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Order
{
    public class TaobaoQueryOrderDetailResponseAmountCountContentData
    {
        public TaobaoQueryOrderDetailResponseAmountCountContentDataMoney money;
        public TaobaoQueryOrderDetailResponseAmountCountContentDataTitle title;
        public TaobaoQueryOrderDetailResponseAmountCountContentDataTitle titleLink;
        public TaobaoQueryOrderDetailResponseAmountCountContentDataMoney dotPrefixMoney;
        public TaobaoQueryOrderDetailResponseAmountCountContentDataMoney dotSufixMoney;
    }
}