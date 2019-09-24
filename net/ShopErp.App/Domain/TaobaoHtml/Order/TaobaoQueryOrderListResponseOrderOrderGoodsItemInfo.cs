using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Order
{
    public class TaobaoQueryOrderListResponseOrderOrderGoodsItemInfo
    {
        public TaobaoQueryOrderListResponseOrderOrderGoodsItemInfoExtra[] extra;

        public string pic;

        public TaobaoQueryOrderListResponseOrderOrderGoodsItemInfoExtra[] skuText;

        public string title;

        public string itemUrl;
    }
}