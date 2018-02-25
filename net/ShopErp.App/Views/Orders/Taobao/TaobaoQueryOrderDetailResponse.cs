using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Views.Orders.Taobao
{
    public class TaobaoQueryOrderDetailResponse
    {
        public TaobaoQueryOrderDetailResponseBasic basic;

        public TaobaoQueryOrderDetailResponseStepbar stepbar;

        public TaobaoQueryOrderDetailResponseOverStatus overStatus;

        public TaobaoQueryOrderDetailResponseAmount amount;

        public TaobaoQueryOrderDetailResponseOrder orders;
    }
}