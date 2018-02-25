using System;

namespace ShopErp.Domain
{
    public class PrintHistory
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ShopId { get; set; }
        public String DeliveryTemplate { get; set; }
        public String DeliveryCompany { get; set; }
        public String DeliveryNumber { get; set; }
        public String Operator { get; set; }
        public String ReceiverName { get; set; }
        public String ReceiverPhone { get; set; }
        public String ReceiverMobile { get; set; }
        public String ReceiverAddress { get; set; }
        public String GoodsInfo { get; set; }
        public String PopOrderId { get; set; }
        public PaperType PaperType { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UploadTime { get; set; }
        public int PageNumber { get; set; }
    }
}
