using System.Linq;
using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    class OrderWeight : IOrderFormatter
    {
        public  string AcceptType
        {
            get { return PrintTemplateItemType.ORDER_WEIGHT; }
        }

        public  object Format(PrintTemplate template, PrintTemplateItem item, Order order)
        {
            if (order.Weight <= 0)
            {
                return "";
            }

            if (order.OrderGoodss == null || order.OrderGoodss.Count < 1)
            {
                return "";
            }

            if (order.OrderGoodss.Any(obj => obj.Weight <= 0))
            {
                return "";
            }

            return order.Weight.ToString("F2");
        }
    }
}
