using System;
using System.Collections.Generic;

namespace ShopErp.Domain
{
    public class GoodsCount
    {
        public bool LianLang { get; set; }

        public string Address { get; set; }

        public string Vendor { get; set; }

        public string Number { get; set; }

        public string Edtion { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public double Money { get; set; }

        public int Count { get; set; }

        public int Area { get; set; }

        public int Street { get; set; }

        public int Door { get; set; }

        public DateTime FirstPayTime { get; set; }

        public DateTime LastPayTime { get; set; }

        public string OrderId { get; set; }

        public long NumberId { get; set; }

        public PopType PopType { get; set; }

        public string DeliveryCompany { get; set; }

        public OrderState State { get; set; }

        public string Comment { get; set; }

        public List<DeliveryCount> DeliveryCounts{ get; set; }
    }
}
