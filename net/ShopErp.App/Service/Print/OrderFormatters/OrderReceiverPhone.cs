using System.Linq;
using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    class OrderReceiverPhone : IOrderFormatter
    {
        public  string AcceptType
        {
            get { return PrintTemplateItemType.ORDER_RECEIVER_PHONE; }
        }

        public  object Format(PrintTemplate template, PrintTemplateItem item, Order order)
        {
            if (item.Format == "否")
                return order.ReceiverPhone;
            return OrderReceiverMobile.Deco(order.ReceiverPhone);
        }
    }
}
