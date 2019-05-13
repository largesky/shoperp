using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OtherFormatters
{
    public class OtherFormatterManager : PrintDataFormatterManagerBase<IOtherFormatter>
    {
        public static object Format(PrintTemplate template, PrintTemplateItem item)
        {
            var formatter = GetPrintDataFormatter(item.Type);
            return formatter.Format(template, item);
        }
    }
}
