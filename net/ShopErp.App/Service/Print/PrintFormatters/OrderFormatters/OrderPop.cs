using ShopErp.App.Domain;
using ShopErp.App.Utils;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    class OrderPop : IOrderFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.ORDER_POP; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, Order order)
        {
            var type = order.PopType;

            if (type == PopType.None)
            {
                return "";
            }

            return EnumUtil.GetEnumValueDescription(type);
        }
    }
}
