using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.ReturnFormatters
{
    public class ReturnFormatterManager : PrintDataFormatterManagerBase<IReturnFormatter>
    {
        public static object Format(PrintTemplate template, PrintTemplateItem item, OrderReturn or)
        {
            return GetPrintDataFormatter(item.Type).Format(template, item, or);
        }
    }
}
