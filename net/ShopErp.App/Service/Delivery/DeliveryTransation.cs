using System.Collections.Generic;

namespace ShopErp.App.Service.Delivery
{
    public class DeliveryTransation
    {
        public bool IsSigned { get; set; }

        public List<DeliveryTransationItem> Items { get; set; }
    }
}