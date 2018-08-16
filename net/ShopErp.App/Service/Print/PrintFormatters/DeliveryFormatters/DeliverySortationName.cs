using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.DeliveryFormatters
{
    class DeliverySortationName : IDeliveryFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.DELIVERY_SORTATIONNAME; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, WuliuNumber wuliuNumber)
        {
            if (wuliuNumber == null || string.IsNullOrWhiteSpace(wuliuNumber.SortationName))
            {
                return "";
            }

            return wuliuNumber.SortationName;
        }
    }
}
