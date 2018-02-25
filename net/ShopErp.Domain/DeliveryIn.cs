using System;

namespace ShopErp.Domain
{
    public class DeliveryIn
    {
        public long Id { get; set; }
        public string DeliveryCompany { get; set; }
        public string DeliveryNumber { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateOperator { get; set; }
        public string Comment { get; set; }
    }
}
