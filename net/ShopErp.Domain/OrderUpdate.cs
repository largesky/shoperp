using System;

namespace ShopErp.Domain
{
    public class OrderUpdate
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string PopOrderId { get; set; }
        public DateTime PrintTime { get; set; }
        public DateTime DeliveryTime { get; set; }
        public OrderState State { get; set; }
    }
}
