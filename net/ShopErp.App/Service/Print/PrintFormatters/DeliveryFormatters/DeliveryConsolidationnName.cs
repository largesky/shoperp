using ShopErp.App.Service.Print.DeliveryFormatters;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintFormatters.DeliveryFormatters
{
    class DeliveryConsolidationnName : IDeliveryFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.DELIVERY_CONSOLIDATIONNAME; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, WuliuNumber wuliuNumber)
        {
            return wuliuNumber.ConsolidationName;
        }
    }
}
