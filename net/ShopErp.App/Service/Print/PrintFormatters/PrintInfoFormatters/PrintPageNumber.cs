using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.PrintFormatters.PrintInfoFormatters
{
    class PrintPageNumber : IPrintInfoFormatter
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
