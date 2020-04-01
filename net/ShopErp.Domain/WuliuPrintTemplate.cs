using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain
{
    public class WuliuPrintTemplate
    {
        /// <summary>
        /// 模板来源
        /// </summary>
        public WuliuPrintTemplateSourceType SourceType { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///快递公司
        /// </summary>
        public string DeliveryCompany { get; set; }

        /// <summary>
        /// 快递公司编码
        /// </summary>
        public string CpCode { get; set; }

        /// <summary>
        /// 打印机名称
        /// </summary>
        public string PrinterName { get; set; }

        /// <summary>
        /// 模板编号
        /// </summary>
        public string StandTemplateId { get; set; }

        /// <summary>
        /// 标准模板URL
        /// </summary>
        public string StandTemplateUrl { get; set; }

        /// <summary>
        /// 如果是用户或者ISV自定义模板则有值
        /// </summary>
        public string UserOrIsvTemplateAreaId { get; set; }

        /// <summary>
        /// 如果是用户或者ISV自定义模板则有值
        /// </summary>
        public string UserOrIsvTemplateAreaUrl { get; set; }

        /// <summary>
        /// 是否是ISV提供的模板
        /// </summary>
        public bool IsIsv { get; set; }

        public int XOffset { get; set; }

        public int YOffset { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
