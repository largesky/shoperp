using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.GoodsFormatters
{
    public interface IGoodsFormatter : PrintDataFormatterBase
    {
        object Format(PrintTemplate template, PrintTemplateItem item, OrderGoods orderGoods);
    }
}
