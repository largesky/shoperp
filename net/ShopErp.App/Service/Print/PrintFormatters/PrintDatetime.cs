using ShopErp.App.Domain;

namespace ShopErp.App.Service.Print.PrintFormatters
{
    class PrintDatetime : IPrintFormatter
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
