using System;
using System.Collections.Generic;

namespace ShopErp.Domain
{
    public class Order
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public OrderCreateType CreateType { get; set; }
        public OrderType Type { get; set; }
        public PopType PopType { get; set; }
        public string PopOrderId { get; set; }
        public string PopBuyerId { get; set; }
        public PopPayType PopPayType { get; set; }
        public float PopCodSevFee { get; set; }
        public string PopCodNumber { get; set; }
        public float PopOrderTotalMoney { get; set; }
        public float PopSellerGetMoney { get; set; }
        public float PopBuyerPayMoney { get; set; }
        public string PopSellerComment { get; set; }
        public string PopBuyerComment { get; set; }
        public string PopState { get; set; }
        public ColorFlag PopFlag { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverMobile { get; set; }
        public string ReceiverAddress { get; set; }
        public long DeliveryTemplateId { get; set; }
        public string DeliveryCompany { get; set; }
        public string DeliveryNumber { get; set; }
        public float DeliveryMoney { get; set; }
        public PaperType PrintPaperType { get; set; }
        public float Weight { get; set; }
        public DateTime PopCreateTime { get; set; }
        public DateTime PopPayTime { get; set; }
        public DateTime PopDeliveryTime { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime PrintTime { get; set; }
        public DateTime DeliveryTime { get; set; }
        public DateTime CloseTime { get; set; }
        public string CreateOperator { get; set; }
        public string PrintOperator { get; set; }
        public string DeliveryOperator { get; set; }
        public string CloseOperator { get; set; }
        public bool ParseResult { get; set; }
        public bool Refused { get; set; }
        public OrderState State { get; set; }
        public IList<OrderGoods> OrderGoodss { get; set; }
    }
}
