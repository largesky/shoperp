using System;

namespace ShopErp.Domain
{
    public class OrderModifyHistory
    {
        public long Id { get; set; }

        public long OrderId { get; set; }

        public string Opeartor { get; set; }

        public DateTime CreateTime { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }
    }
}
