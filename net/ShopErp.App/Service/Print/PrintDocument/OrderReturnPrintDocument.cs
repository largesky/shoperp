using ShopErp.App.Service.Print.OtherFormatters;
using ShopErp.App.Service.Print.PrintFormatters.PrintInfoFormatters;
using ShopErp.App.Service.Print.ReturnFormatters;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument
{
    class OrderReturnPrintDocument : GdiPrintDocumentBase<OrderReturn>
    {
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
            else if (group == PrintTemplateItemTypeGroup.RETURN)
            {
                data = ReturnFormatterManager.Format(this.Template, printTemplateItem, this.Values[index]);
            }
            else
            {
                throw new Exception("商品模板不支类型:" + printTemplateItem.Type);
            }
            return data;
        }
    }
}
