using ShopErp.App.Domain;

namespace ShopErp.App.Service.Print.OtherFormatters
{
    class OtherStaticText : IOtherFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.OTHER_STATICTEXT; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item)
        {
            return item.Format;
        }
    }
}
