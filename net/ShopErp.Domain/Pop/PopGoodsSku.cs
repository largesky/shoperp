namespace ShopErp.Domain.Pop
{
    public class PopGoodsSku
    {
        /// <summary>
        /// sku的图片
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// SKU 在对就平台的ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///  厂家与商品的完整编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// 尺码
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// SKU数量
        /// </summary>
        public string Stock { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public PopGoodsState Status { get; set; }
    }
}
