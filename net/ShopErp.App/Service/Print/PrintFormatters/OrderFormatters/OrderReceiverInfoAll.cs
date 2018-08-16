using System;
using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    class OrderReceiverInfoAll : IOrderFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.ORDER_RECEIVER_INFOALL; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, Order order)
        {
            string phone = item.Format != "否" ? OrderReceiverMobile.Deco(order.ReceiverPhone) : order.ReceiverPhone;
            string mobile = item.Format != "否" ? OrderReceiverMobile.Deco(order.ReceiverMobile) : order.ReceiverMobile;
            string s = string.Join("  ", order.ReceiverName, mobile, phone);
            s += Environment.NewLine + order.ReceiverAddress;
            return s;
        }
    }
}
