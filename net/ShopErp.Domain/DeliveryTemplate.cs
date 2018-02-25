using System;
using System.Collections.Generic;

namespace ShopErp.Domain
{
    public class DeliveryTemplate
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string DeliveryCompany { get; set; }

        public bool HotPaperUse { get; set; }

        public bool NormalPaperUse { get; set; }

        public bool OnlinePayTypeUse { get; set; }

        public bool CodPayTypeUse { get; set; }

        public float EmptyHotPaperMoney { get; set; }

        public float EmptyNormalPaperMoney { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public string UpdateOperator { get; set; }

        public IList<DeliveryTemplateArea> Areas { get; set; }
    }
}
