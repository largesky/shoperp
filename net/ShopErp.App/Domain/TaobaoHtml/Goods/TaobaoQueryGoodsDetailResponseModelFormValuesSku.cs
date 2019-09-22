using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Goods
{
    class TaobaoQueryGoodsDetailResponseModelFormValuesSku
    {
        public string skuId;
        public float skuPrice;
        public int skuStock;
        public string skuOuterId;

        public Image[] skuImage;

        public TaobaoQueryGoodsDetailResponseModelFormValuesSkuProp[] props;
    }
}
