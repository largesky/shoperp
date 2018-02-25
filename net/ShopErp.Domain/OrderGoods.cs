using System;

namespace ShopErp.Domain
{

    public class OrderGoods
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string Vendor { get; set; }
        public string Number { get; set; }
        public string PopNumber { get; set; }
        public long NumberId { get; set; }
        public string Edtion { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public int Count { get; set; }
        public int GetedCount { get; set; }
        public float Price { get; set; }
        public float PopPrice { get; set; }
        public string PopUrl { get; set; }
        public string PopInfo { get; set; }
        public string PopOrderSubId { get; set; }
        public DateTime CloseTime { get; set; }
        public string CloseOperator { get; set; }
        public string Comment { get; set; }
        public DateTime StockTime { get; set; }
        public string StockOperator { get; set; }
        public string Image { get; set; }
        public OrderState State { get; set; }
        public PopRefundState PopRefundState { get; set; }
        public float Weight { get; set; }
        public bool IsPeijian { get; set; }
    }
}
