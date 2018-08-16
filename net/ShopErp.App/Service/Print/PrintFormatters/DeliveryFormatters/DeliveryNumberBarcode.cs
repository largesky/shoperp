using System;
using ShopErp.App.Domain;
using ShopErp.App.Utils;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.DeliveryFormatters
{
    class DeliveryNumberBarcode : IDeliveryFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.DELIVERY_DELIVERYNUMBERBARCODE; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, WuliuNumber wuliuNumber)
        {
            if (string.IsNullOrWhiteSpace(wuliuNumber.DeliveryNumber))
            {
                throw new Exception("快递单号为空");
            }
            return ZXingUtil.CreateImage(wuliuNumber.DeliveryNumber, item.Format, (int)item.Width, (int)item.Height, item.Value1 == "是" ? false : true, item.FontName, (float)item.FontSize);
        }
    }
}
