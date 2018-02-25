using System;

namespace ShopErp.Domain
{
    public class GoodsShop
    {
        public long Id { get; set; }

        public long GoodsId { get; set; }

        public long ShopId { get; set; }

        public GoodsState State { get; set; }

        public float SalePrice { get; set; }

        public DateTime ProcessImageTime { get; set; }

        public DateTime UploadTime { get; set; }

        public string ProcessImageOperator { get; set; }

        public string UploadOperator { get; set; }

        public string PopGoodsId { get; set; }
    }
}
