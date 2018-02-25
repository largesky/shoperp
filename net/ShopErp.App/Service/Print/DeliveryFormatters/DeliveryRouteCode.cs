using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.DeliveryFormatters
{
    class DeliveryRouteCode : IDeliveryFormatter
    {
        public string AcceptType
        {
            get
            {
                return PrintTemplateItemType.DELIVERY_ROUTECODE;
            }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, WuliuNumber wuliuNumber)
        {
            string routeCode = wuliuNumber.RouteCode;
            if (string.IsNullOrWhiteSpace(routeCode))
            {
                return null;
            }

            if (item.Format == "全部")
            {
                return routeCode;
            }

            string[] codes = routeCode.Split(new char[] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (item.Format == "第一段")
            {
                return codes[0];
            }
            if (item.Format == "第二段")
            {
                return codes.Length >= 2 ? codes[1] : codes[0];
            }

            return codes.Last();
        }
    }
}
