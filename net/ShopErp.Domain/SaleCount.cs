using System;

namespace ShopErp.Domain
{
    public class SaleCount
    {
        public long OrderId { get; set; }

        public long OrderGoodsId { get; set; }

        public string Image { get; set; }

        public int Count { get; set; }

        public float PopPrice { get; set; }

        public float PopSellerGetMoney { get; set; }

        public float ERPOrderGoodsMoney { get; set; }

        public float ERPOrderDeliveryMoney { get; set; }

        public string Vendor { get; set; }

        public string Number { get; set; }

        public long GoodsId { get; set; }

        public DateTime PopPayTime { get; set; }

        public DateTime DeliveryTime { get; set; }

        public OrderState State { get; set; }

        public long ShopId { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public string Edtion { get; set; }

        /// <summary>
        /// 活动扣点
        /// </summary>
        public float Points { get; set; }
    }
}
