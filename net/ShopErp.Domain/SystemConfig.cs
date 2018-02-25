using System;

namespace ShopErp.Domain
{
    public class SystemConfig
    {
        public long Id { get; set; }

        public long OwnerId { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public string UpdateOperator { get; set; }
    }
}
