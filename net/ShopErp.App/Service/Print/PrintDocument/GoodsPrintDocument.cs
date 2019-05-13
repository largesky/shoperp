using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ShopErp.App.Service.Print;
using ShopErp.App.Service.Print.GoodsFormatters;
using ShopErp.App.Service.Print.OtherFormatters;
using ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument;
using ShopErp.App.Service.Print.PrintFormatters;
using ShopErp.App.Service.Print.PrintFormatters.PrintInfoFormatters;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.PrintDocument
{
    class GoodsPrintDocument : GdiPrintDocumentBase<OrderGoods>
    {
        private List<DocumentPage> pages = new List<DocumentPage>();

        protected override object FormatData(PrintTemplateItem printTemplateItem, PrintInfo pi)
        {
            //该用那个格式货程序
            var group = PrintTemplateItemType.GetGroup(printTemplateItem.Type);
            object data = null;
            if (group == PrintTemplateItemTypeGroup.PRINT)
            {
                data = PrintFormatterManager.Format(this.Template, printTemplateItem, pi);
            }
            else if (group == PrintTemplateItemTypeGroup.OTHER)
            {
                data = OtherFormatterManager.Format(this.Template, printTemplateItem);
            }
            else if (group == PrintTemplateItemTypeGroup.GOODS)
            {
                data = GoodsFormatterManager.Format(this.Template, printTemplateItem, this.Values[this.index]);
            }
            else
            {
                throw new Exception("商品模板不支类型:" + printTemplateItem.Type);
            }
            return data;
        }
    }
}