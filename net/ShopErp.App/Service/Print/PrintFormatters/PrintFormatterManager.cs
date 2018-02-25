using ShopErp.App.Domain;

namespace ShopErp.App.Service.Print.PrintFormatters
{
    public class PrintFormatterManager : PrintDataFormatterManagerBase<IPrintFormatter>
    {
        public static object Format(PrintTemplate template, PrintTemplateItem item, PrintInfo printInfo)
        {
            var formtter = GetPrintDataFormatter(item.Type);
            return formtter.Format(template, item, printInfo);
        }
    }
}
