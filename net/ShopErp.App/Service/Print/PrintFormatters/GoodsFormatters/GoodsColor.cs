using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.GoodsFormatters
{
    public class GoodsColor : IGoodsFormatter
    {
        public string AcceptType { get { return PrintTemplateItemType.GOODS_COLOR; } }

        public object Format(PrintTemplate template, PrintTemplateItem item, OrderGoods orderGoods)
        {
            return "颜色:" + orderGoods.Color;
        }
    }
}
