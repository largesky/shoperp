using System;
using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.DeliveryFormatters
{
    public class DeliveryFormatterManager : PrintDataFormatterManagerBase<IDeliveryFormatter>
    {
        public static object Format(PrintTemplate template, PrintTemplateItem item, WuliuNumber wuliuNumber)
        {
            if (template == null)
            {
                throw new Exception("打印模板为空");
            }

            var formatter = GetPrintDataFormatter(item.Type);
            return formatter.Format(template, item, wuliuNumber);
        }
    }
}
