using ShopErp.Domain.Common;
using System.Collections.Generic;

namespace ShopErp.Domain.Pop
{
    public class PopGoods
    {
        /// <summary>
        /// 商品在对应平台的ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 商品标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 商品的商家编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string AddTime { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public string UpdateTime { get; set; }

        /// <summary>
        /// 已卖数量
        /// </summary>
        public int SaleNum { get; set; }

        /// <summary>
        /// 第一张图片
        /// </summary>
        public string Image
        {
            get { return Images != null && Images.Length > 0 ? Images[0] : ""; }
        }

        /// <summary>
        /// 商品在对应平台的目录ID
        /// </summary>
        public string CatId { get; set; }

        /// <summary>
        /// 商品状态 
        /// </summary>
        public PopGoodsState State { get; set; }

        /// <summary>
        /// 商品类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 商品所在平台
        /// </summary>
        public PopType PopType { get; set; }

        /// <summary>
        /// 商品主图
        /// </summary>
        public string[] Images { get; set; }

        /// <summary>
        /// 商品详情图
        /// </summary>
        public string[] DescImages { get; set; }

        /// <summary>
        /// SKU
        /// </summary>
        public List<PopGoodsSku> Skus { get; set; }

        /// <summary>
        /// 相关属性，或者一个属性是多选的，则用使用@#@分开
        /// </summary>
        public List<KeyValuePairClass<string, string>> Properties { get; set; }

        /// <summary>
        /// 发货城市
        /// </summary>
        public string ShippingCity { get; set; }

        public PopGoods()
        {
            this.Skus = new List<PopGoodsSku>();
            this.Properties = new List<KeyValuePairClass<string, string>>();
        }
    }
}
