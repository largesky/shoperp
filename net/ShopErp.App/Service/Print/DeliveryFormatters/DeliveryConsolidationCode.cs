using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.DeliveryFormatters
{
    class DeliveryConsolidationCode : IDeliveryFormatter
    {
        public  string AcceptType
        {
            get { return PrintTemplateItemType.DELIVERY_CONSOLIDATIONCODE; }
        }

        public  object Format(PrintTemplate template, PrintTemplateItem item,  WuliuNumber wuliuNumber)
        {
            return wuliuNumber.ConsolidationCode;
        }
    }
}
