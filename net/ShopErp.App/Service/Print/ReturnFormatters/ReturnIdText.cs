using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.ReturnFormatters
{
    class ReturnIdText : IReturnFormatter
    {
        public string AcceptType { get { return PrintTemplateItemType.RETURN_ORDERCHANDE_ID_TEXT; } }

        public object Format(PrintTemplate template, PrintTemplateItem item, OrderReturn or)
        {
            return or.Id.ToString();
        }
    }
}
