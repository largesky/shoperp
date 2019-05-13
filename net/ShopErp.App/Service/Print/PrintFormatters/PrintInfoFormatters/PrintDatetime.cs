using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.PrintFormatters.PrintInfoFormatters
{
    class PrintDatetime : IPrintInfoFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.PRINT_DATETIME; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, PrintInfo printInfo)
        {
            return printInfo.PrintTime.ToString(item.Format);
        }
    }
}
