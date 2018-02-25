using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.ReturnFormatters
{
    public interface IReturnFormatter : PrintDataFormatterBase
    {
        object Format(PrintTemplate template, PrintTemplateItem item, OrderReturn or);
    }
}
