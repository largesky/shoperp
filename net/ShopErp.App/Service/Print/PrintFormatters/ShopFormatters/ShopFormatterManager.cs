using ShopErp.App.Domain;

namespace ShopErp.App.Service.Print.ShopFormatters
{
    public class ShopFormatterManager : PrintDataFormatterManagerBase<IShopFormatter>
    {
        public static object Format(PrintTemplate template, PrintTemplateItem item, long shopId)
        {
            var formatter = GetPrintDataFormatter(item.Type);
            return formatter.Format(template, item, shopId);
        }
    }
}
