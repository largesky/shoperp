using System;

namespace ShopErp.Domain
{
    public class WuliuNumber
    {
        public long Id { get; set; }

        public WuliuPrintTemplateSourceType SourceType { get; set; }

        public string WuliuIds { get; set; }

        public string ReceiverName { get; set; }

        public string ReceiverPhone { get; set; }

        public string ReceiverMobile { get; set; }

        public string ReceiverAddress { get; set; }

        public string PackageId { get; set; }

        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 快递公司
        /// </summary>
        public string DeliveryCompany { get; set; }

        /// <summary>
        /// 快递单号
        /// </summary>
        public string DeliveryNumber { get; set; }

        /// <summary>
        /// 菜鸟返回的打印JSON数据,不保存这个字段到数据库
        /// </summary>
        public string PrintData { get; set; }
    }
}
