using System.Linq;
using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    class OrderGoodsCount : IOrderFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.ORDER_GOODS_COUNT; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, Order order)
        {
            if (order.OrderGoodss == null || order.OrderGoodss.Count < 1)
            {
                return "";
            }

            var goods = order.OrderGoodss.Where(obj => (int)obj.State <= (int)OrderState.SHIPPED);
            var count = goods.Select(obj => obj.Count).Sum();
            return count.ToString();
        }
    }
}
