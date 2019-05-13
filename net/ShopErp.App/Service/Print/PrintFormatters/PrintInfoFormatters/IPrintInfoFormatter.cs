using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.PrintFormatters.PrintInfoFormatters
{
    public interface IPrintInfoFormatter : PrintDataFormatterBase
    {
        object Format(PrintTemplate template, PrintTemplateItem item, PrintInfo printInfo);
    }
}
