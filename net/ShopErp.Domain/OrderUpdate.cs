using System;

namespace ShopErp.Domain
{
    public class OrderUpdate
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string PopOrderId { get; set; }
        public float PopCodSevFee { get; set; }
        public float PopOrderTotalMoney { get; set; }
        public string PopState { get; set; }
        public string PopCodNumber { get; set; }
        public DateTime PopPayTime { get; set; }
        public DateTime PrintTime { get; set; }
        public DateTime DeliveryTime { get; set; }
        public OrderState State { get; set; }
    }
}
