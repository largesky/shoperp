using ShopErp.App.Domain;
using ShopErp.App.Utils;

namespace ShopErp.App.Service.Print.OtherFormatters
{
    class OtherBarcode : IOtherFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.OTHER_BARCODE; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item)
        {
            return ZXingUtil.CreateImage(item.Value, item.Format, (int)item.Width, (int)item.Height, item.Value1 == "是" ? false : true, item.FontName, (float)item.FontSize);
        }
    }
}
