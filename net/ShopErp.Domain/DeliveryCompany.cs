using System;

namespace ShopErp.Domain
{
    public class DeliveryCompany
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool PaperMark { get; set; }

        public string PopMapKuaidi100 { get; set; }

        public string PopMapTaobao { get; set; }

        public string PopMapPingduoduo { get; set; }

        public string PopMapJd { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public string UpdateOperator { get; set; }
    }
}
