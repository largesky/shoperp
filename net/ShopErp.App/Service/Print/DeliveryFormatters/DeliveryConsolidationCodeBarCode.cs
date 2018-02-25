using ShopErp.App.Domain;
using ShopErp.App.Utils;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.DeliveryFormatters
{
    class DeliveryConsolidationCodeBarCode : IDeliveryFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.DELIVERY_CONSOLIDATIONCODE_BARCODE; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, WuliuNumber wuliuNumber)
        {
            if (string.IsNullOrWhiteSpace(wuliuNumber.ConsolidationCode))
            {
                return "";
            }

            return ZXingUtil.CreateImage(wuliuNumber.ConsolidationCode, item.Format, (int)item.Width, (int)item.Height, item.Value1 == "是" ? false : true, item.FontName, (float)item.FontSize);
        }
    }
}
