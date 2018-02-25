using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    class OrderReceiverName : IOrderFormatter
    {
        public  string AcceptType
        {
            get { return PrintTemplateItemType.ORDER_RECEIVER_NAME; }
        }

        public  object Format(PrintTemplate template, PrintTemplateItem item, Order order)
        {
            return order.ReceiverName.Trim();
        }
    }
}
