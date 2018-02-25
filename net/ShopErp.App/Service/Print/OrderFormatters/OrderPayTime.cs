using System;
using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    class OrderPayTime : IOrderFormatter
    {
        private DateTime dateTime = new DateTime(2000, 01, 01);

        public string AcceptType
        {
            get { return PrintTemplateItemType.ORDER_PAYTIME; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, Order order)
        {
            if (order.PopPayTime <= dateTime)
            {
                return order.CreateTime.ToString(item.Format);
            }
            return order.PopPayTime.ToString(item.Format);
        }
    }
}
