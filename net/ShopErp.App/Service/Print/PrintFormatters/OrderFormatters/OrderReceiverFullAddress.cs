using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    class OrderReceiverFullAddress : IOrderFormatter
    {
        public  string AcceptType
        {
            get { return PrintTemplateItemType.ORDER_RECEIVER_ADDRESS; }
        }

        public  object Format(PrintTemplate template, PrintTemplateItem item, Order order)
        {
            return order.ReceiverAddress;
        }
    }
}
