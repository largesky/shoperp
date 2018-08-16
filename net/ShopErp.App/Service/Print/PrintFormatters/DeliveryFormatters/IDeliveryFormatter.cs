using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.DeliveryFormatters
{
    public interface IDeliveryFormatter : PrintDataFormatterBase
    {
        object Format(PrintTemplate template, PrintTemplateItem item, WuliuNumber wuliuNumber);
    }
}
