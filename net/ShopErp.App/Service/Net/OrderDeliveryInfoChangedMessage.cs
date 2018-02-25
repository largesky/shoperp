using System;

namespace ShopErp.App.Service.Net
{
    [Serializable]
    public class OrderDeliveryInfoChangedMessage : Message
    {
        public long OrderId { get; set; }

        public string DeliveryCompany { get; set; }

        public string DeliveryNumber { get; set; }
    }
}