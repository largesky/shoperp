using ShopErp.App.Domain;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.GoodsFormatters
{
    public class GoodsMaterial : IGoodsFormatter
    {
        public string AcceptType { get { return PrintTemplateItemType.GOODS_MEATERIAL; } }

        public object Format(PrintTemplate template, PrintTemplateItem item, OrderGoods orderGoods)
        {
            string ma = "材质:";
            if (orderGoods.GoodsId < 1)
            {
                return ma;
            }
            var gu = ServiceContainer.GetService<GoodsService>().GetById(orderGoods.GoodsId);
            if (gu != null)
            {
                return ma + gu.Material;
            }

            return "";
        }
    }
}
