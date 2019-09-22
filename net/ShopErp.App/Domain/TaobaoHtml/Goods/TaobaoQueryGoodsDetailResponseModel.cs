using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Goods
{
    class TaobaoQueryGoodsDetailResponseModel
    {

        /// <summary>
        /// 淘宝需要的下面地址
        /// </summary>
        public TaobaoQueryGoodsDetailResponseModelGlobal global;

        public TaobaoQueryGoodsDetailResponseModelFormValues formValues;

        public TaobaoQueryGoodsDetailResponseModelProp itemProp;

        public TaobaoQueryGoodsDetailResponseModelProp bindProp;


        /// <summary>
        /// 淘宝只有一个属性组
        /// </summary>
        public TaobaoQueryGoodsDetailResponseModelProp catProp;

        public TaobaoQueryGoodsDetailResponseModelCatPath catpath;

    }
}
