using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    class OrderPopPayType : IOrderFormatter
    {
        public  string AcceptType
        {
            get { return PrintTemplateItemType.ORDER_POPPAYTYPE; }
        }

        public  object Format(PrintTemplate template, PrintTemplateItem item, Order order)
        {
            if (order.PopPayType ==PopPayType.COD)
            {
                return "代收货款";
            }
            return "";
        }
    }
}
