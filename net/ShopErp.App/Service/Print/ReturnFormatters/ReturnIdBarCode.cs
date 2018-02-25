using ShopErp.App.Domain;
using ShopErp.App.Utils;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.ReturnFormatters
{
    class ReturnIdBarCode : IReturnFormatter
    {
        public string AcceptType { get { return PrintTemplateItemType.RETURN_ORDERCHANDE_ID_BARCODE; } }

        public object Format(PrintTemplate template, PrintTemplateItem item, OrderReturn or)
        {
            return ZXingUtil.CreateImage(or.Id.ToString(), item.Format, (int)item.Width, (int)item.Height, item.Value1 == "是" ? false : true, item.FontName, (float)item.FontSize);
        }
    }
}
