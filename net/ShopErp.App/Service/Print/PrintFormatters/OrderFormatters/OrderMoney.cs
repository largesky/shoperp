using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    class OrderMoney : IOrderFormatter
    {
        public  string AcceptType
        {
            get { return PrintTemplateItemType.ORDER_MONEY; }
        }

        public  object Format(PrintTemplate template, PrintTemplateItem item, Order order)
        {
            return order.PopOrderTotalMoney.ToString("F2") + "￥";
        }
    }
}
