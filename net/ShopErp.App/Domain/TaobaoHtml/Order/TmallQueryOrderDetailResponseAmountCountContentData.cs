using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Order
{
    public class TmallQueryOrderDetailResponseAmountCountContentData
    {
        public TmallQueryOrderDetailResponseAmountCountContentDataMoney money;
        public TmallQueryOrderDetailResponseAmountCountContentDataTitle title;
        public TmallQueryOrderDetailResponseAmountCountContentDataTitle titleLink;
        public TmallQueryOrderDetailResponseAmountCountContentDataMoney dotPrefixMoney;
        public TmallQueryOrderDetailResponseAmountCountContentDataMoney dotSufixMoney;
    }
}