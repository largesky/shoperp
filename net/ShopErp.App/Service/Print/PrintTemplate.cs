using System;
using System.Collections.Generic;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print
{
    /// <summary>
    /// 所有打印模板的基类
    /// </summary>
    [Serializable]
    public class PrintTemplate
    {
        public const string TYPE_DELIVER = "快递";

        public const string TYPE_GOODS = "商品";

        public const string TYPE_RETURN = "退货";


        /// <summary>
        /// 模板类型
        /// </summary>
        public string Type { get; set; }


        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        ///快递公司
        /// </summary>
        public string DeliveryCompany { get; set; }

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
        /// 背景图片
        /// </summary>
        public byte[] BackgroundImage { get; set; }

        /// <summary>
        /// 其它图片
        /// </summary>
        public Dictionary<string, byte[]> AttachFiles { get; set; }

        public PrintTemplate()
        {
            this.Items = new List<PrintTemplateItem>();
            this.AttachFiles = new Dictionary<string, byte[]>();
        }
    }
}
