using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.GoodsFormatters
{
    public class GoodsFormatterManager : PrintDataFormatterManagerBase<IGoodsFormatter>
    {
        public static object Format(PrintTemplate template, PrintTemplateItem item, OrderGoods orderGoods)
        {
            var formtter = GetPrintDataFormatter(item.Type);
            return formtter.Format(template, item, orderGoods);
        }
    }
}
