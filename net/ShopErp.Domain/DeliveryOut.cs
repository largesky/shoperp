using System;

namespace ShopErp.Domain
{

    public class DeliveryOut
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string OrderId { get; set; }
        public PopType PopType { get; set; }
        public PopPayType PopPayType { get; set; }
        public string DeliveryCompany { get; set; }
        public string DeliveryNumber { get; set; }
        public string ReceiverAddress { get; set; }
        public float Weight { get; set; }
        public float ERPDeliveryMoney { get; set; }
        public float ERPGoodsMoney { get; set; }
        public float PopDeliveryMoney { get; set; }
        public float PopCodSevFee { get; set; }
        public float PopGoodsMoney { get; set; }
        public string GoodsInfo { get; set; }
        public string Operator { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
