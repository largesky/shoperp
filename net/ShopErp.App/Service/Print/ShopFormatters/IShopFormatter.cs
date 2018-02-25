using ShopErp.App.Domain;

namespace ShopErp.App.Service.Print.ShopFormatters
{
    public interface IShopFormatter : PrintDataFormatterBase
    {
        object Format(PrintTemplate template, PrintTemplateItem item, long shopId);
    }
}
