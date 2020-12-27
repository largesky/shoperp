using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Goods
{
    class TaobaoQueryGoodsDetailResponseModelFormValues
    {

        [JsonConverter(typeof(TitleConverter))]
        public string title;

        public string outerId;

        /// <summary>
        /// 商品主图
        /// </summary>
        public Image[] images;

        /// <summary>
        /// 天猫商品基本属性
        /// </summary>
        [JsonConverter(typeof(ValueTextArrayConverter))]
        public ValueTextArray[] bindProp;

        /// <summary>
        /// 开猫扩展的商品属性
        /// </summary>
        [JsonConverter(typeof(ValueTextArrayConverter))]
        public ValueTextArray[] itemProp;

        /// <summary>
        /// SKU 信息，淘宝和天猫一样的
        /// </summary>
        public TaobaoQueryGoodsDetailResponseModelFormValuesSku[] sku;

        ///// <summary>
        ///// 淘宝商品基本属性
        ///// </summary>
        [JsonConverter(typeof(ValueTextArrayConverter))]
        public ValueTextArray[] catProp;

        /// <summary>
        /// 销售属性，储存是颜色，尺码信息
        /// </summary>
        public TaobaoQueryGoodsDetailResponseModelFormValuesSaleProp saleProp;

        public TaobaoQueryGoodsDetailResponseModelFormValuesLocation location;

        /// <summary>
        /// 商品PC端描述,淘宝
        /// </summary>
        public string desc;

        /// <summary>
        /// 商品PC端描述,天猫
        /// </summary>
        public TaobaoQueryGoodsDetailResponseModelFormValuesModularDesc[] modularDesc;


    }
}
