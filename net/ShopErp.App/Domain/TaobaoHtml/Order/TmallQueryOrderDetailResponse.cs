using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Order
{
    public class TmallQueryOrderDetailResponse
    {
        public TmallQueryOrderDetailResponseBasic basic;

        public TmallQueryOrderDetailResponseStepbar stepbar;

        public TmallQueryOrderDetailResponseOverStatus overStatus;

        public TmallQueryOrderDetailResponseAmount amount;

        public TmallQueryOrderDetailResponseOrder orders;
    }
}