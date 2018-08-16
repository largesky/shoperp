using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    class OrderReceiverMobile : IOrderFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.ORDER_RECEIVER_MOBILE; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, Order order)
        {
            //没有手机，则返回座机
            if (string.IsNullOrWhiteSpace(order.ReceiverMobile))
            {
                if (item.Format == "否")
                    return order.ReceiverPhone;
                return OrderReceiverMobile.Deco(order.ReceiverPhone);
            }

            if (item.Format == "否")
                return order.ReceiverMobile;

            return OrderReceiverMobile.Deco(order.ReceiverMobile);
        }

        public static string Deco(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return string.Empty;
            }

            if (phone.Length == 11)
            {
                return phone.Substring(0, 3) + "****" + phone.Substring(7);
            }

            if (phone.Length < 4)
            {
                return phone;
            }

            return phone.Substring(0, phone.Length - 4) + "****";

        }
    }
}
