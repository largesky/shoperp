using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.DeliveryFormatters
{
    class DeliverySortationNameAndRouteCode : IDeliveryFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.DELIVERY_SORTATIONNAMEANDROUTECODE; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, WuliuNumber wuliuNumber)
        {
            return wuliuNumber.SortationNameAndRouteCode;
        }
    }
}
