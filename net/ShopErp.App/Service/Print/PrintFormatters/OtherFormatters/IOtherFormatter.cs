using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OtherFormatters
{
    public interface IOtherFormatter : PrintDataFormatterBase
    {
        object Format(PrintTemplate template, PrintTemplateItem item);
    }
}
