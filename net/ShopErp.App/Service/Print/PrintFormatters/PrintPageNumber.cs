using ShopErp.App.Domain;

namespace ShopErp.App.Service.Print.PrintFormatters
{
    class PrintPageNumber : IPrintFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.PRINT_PAGENUMBER; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, PrintInfo printInfo)
        {
            return printInfo.PageInfo;
        }
    }
}
