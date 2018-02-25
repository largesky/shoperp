using System;

namespace ShopErp.Domain
{
    public class Finance
    {
        public long Id { get; set; }

        public long FinaceAccountId { get; set; }

        public string Type { get; set; }

        public float Money { get; set; }

        public string Opposite { get; set; }

        public string Comment { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateOperator { get; set; }
    }
}
