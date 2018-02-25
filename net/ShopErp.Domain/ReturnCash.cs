using System;

namespace ShopErp.Domain
{
    public class ReturnCash
    {
        public long Id { get; set; }

        public long ShopId { get; set; }

        public long OrderId { get; set; }

        public string Type { get; set; }

        public string PopOrderId { get; set; }

        public string AccountType { get; set; }

        public string AccountInfo { get; set; }

        public float Money { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateOperator { get; set; }

        public DateTime ProcessTime { get; set; }

        public string ProcessOperator { get; set; }

        public string Comment { get; set; }

        public ReturnCashState State { get; set; }

        public string SerialNumber { get; set; }

        public byte[] Image { get; set; }
    }
}
