using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Goods
{
    class TaobaoQueryGoodsDetailErrorResponse
    {
        public bool success = true;

        public string code;

        public string msg;

        public TaobaoQueryGoodsDetailErrorResponseData data;
    }
}
