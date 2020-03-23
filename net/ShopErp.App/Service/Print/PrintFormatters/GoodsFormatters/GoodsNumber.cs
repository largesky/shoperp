using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.GoodsFormatters
{
    public class GoodsNumber : IGoodsFormatter
    {
        public  string AcceptType { get { return PrintTemplateItemType.GOODS_NUMBER; } }

        public  object Format(PrintTemplate template, PrintTemplateItem item, OrderGoods orderGoods)
        {
            return "货号:" + (orderGoods.GoodsId > 0 ? orderGoods.Number : "");
        }
    }
}
