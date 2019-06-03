using System;
using System.Collections.Generic;
using ShopErp.Domain;

namespace ShopErp.Domain
{
    /// <summary>
    /// 所有打印模板的基类
    /// </summary>
    public class PrintTemplate
    {
        public const string TYPE_DELIVER = "快递";

        public const string TYPE_GOODS = "商品";

        public const string TYPE_RETURN = "退货";


        /// <summary>
        /// 模板类型,是快递单，商品，还是退货
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 模板来源
        /// </summary>
        public PrintTemplateSourceType SourceType { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///快递公司
        /// </summary>
        public string DeliveryCompany { get; set; }

        /// <summary>
        /// 快递公司编码，如果是像菜鸟或者拼多多上下载的才有，这个值
        /// </summary>
        public string CpCode { get; set; }

        /// <summary>
        /// 打印机名称
        /// </summary>
        public string PrinterName { get; set; }

        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 水平偏移
        /// </summary>
        public double XOffset { get; set; }

        /// <summary>
        /// 垂直偏移
        /// </summary>
        public double YOffset { get; set; }

        /// <summary>
        /// 打印项
        /// </summary>
        public List<PrintTemplateItem> Items { get; private set; }

        /// <summary>
        /// 其它图片
        /// </summary>
        public List<PrintTemplateAttachFiles> AttachFiles { get; set; }

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

        public PrintTemplate()
        {
            this.Items = new List<PrintTemplateItem>();
            this.AttachFiles = new List<PrintTemplateAttachFiles>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
