using ShopErp.App.Service.Print;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.PrintFormatters.PrintInfoFormatters
{
    public class PrintFormatterManager : PrintDataFormatterManagerBase<IPrintInfoFormatter>
    {
        public static object Format(PrintTemplate template, PrintTemplateItem item, PrintInfo printInfo)
        {
            var formtter = GetPrintDataFormatter(item.Type);
            return formtter.Format(template, item, printInfo);
        }
    }
}
