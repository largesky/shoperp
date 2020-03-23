using System;
using System.Collections.Generic;

namespace ShopErp.Domain
{
    public class Goods
    {
        public long Id { get; set; }
        public long VendorId { get; set; }
        public GoodsType Type { get; set; }
        public string Image { get; set; }
        public string ImageDir { get; set; }
        public string Comment { get; set; }
        public string Colors { get; set; }
        public float Price { get; set; }
        public string Number { get; set; }
        public string Url { get; set; }
        public float Weight { get; set; }
        public string Material { get; set; }
        public int Star { get; set; }
        public bool IgnoreEdtion { get; set; }
        public bool UpdateEnabled { get; set; }
        public ColorFlag Flag { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string CreateOperator { get; set; }
        public GoodsVideoType VideoType { get; set; }
        public string Shipper { get; set; }
        public IList<GoodsShop> Shops { get; set; }
        //这个字段用于HIBERNATE数据查询使用
        protected virtual Vendor Vendor { get; set; }
    }
}
