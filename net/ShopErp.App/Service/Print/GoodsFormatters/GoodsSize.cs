using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.GoodsFormatters
{
    public class GoodsSize : IGoodsFormatter
    {
        public  string AcceptType { get { return PrintTemplateItemType.GOODS_SIZE; } }

        public  object Format(PrintTemplate template, PrintTemplateItem item, OrderGoods orderGoods)
        {
            return "尺码:" + (220 + (int.Parse(orderGoods.Size) - 34) * 5).ToString() + "/1.5";
        }
    }
}
