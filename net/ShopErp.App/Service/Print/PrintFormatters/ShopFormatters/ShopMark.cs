using ShopErp.App.Domain;
using ShopErp.App.Service.Restful;

namespace ShopErp.App.Service.Print.ShopFormatters
{
    class ShopMark : IShopFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.SHOP_MARK; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, long shopId)
        {
            var shop = ServiceContainer.GetService<ShopService>().GetById(shopId);
            if (shop == null)
            {
                return "";
            }
            return shop.Mark;
        }
    }
}
