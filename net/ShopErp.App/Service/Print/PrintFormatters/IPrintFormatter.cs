using ShopErp.App.Domain;

namespace ShopErp.App.Service.Print.PrintFormatters
{
    public interface IPrintFormatter : PrintDataFormatterBase
    {
        object Format(PrintTemplate template, PrintTemplateItem item, PrintInfo printInfo);
    }
}
