using System;

namespace ShopErp.Domain
{
    public class OrderReturn
    {
        public long Id { get; set; }

        public long NewOrderId { get; set; }

        public long OrderId { get; set; }

        public long OrderGoodsId { get; set; }

        public string GoodsInfo { get; set; }

        public int Count { get; set; }

        public OrderReturnType Type { get; set; }

        public OrderReturnReason Reason { get; set; }

        public string DeliveryCompany { get; set; }

        public string DeliveryNumber { get; set; }

        public string Comment { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime ProcessTime { get; set; }

        public string CreateOperator { get; set; }

        public string ProcessOperator { get; set; }

        public float GoodsMoney { get; set; }

        public OrderReturnState State { get; set; }
    }
}
