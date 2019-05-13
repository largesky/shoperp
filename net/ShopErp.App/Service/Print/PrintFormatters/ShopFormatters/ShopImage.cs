using System;
using System.IO;
using System.Windows.Media.Imaging;
using ShopErp.App.Domain;
using ShopErp.App.Service.Restful;
using ShopErp.App.Utils;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.ShopFormatters
{
    class ShopImage : IShopFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.SHOP_SHOPIMAGE; }
        }


        public object Format(PrintTemplate template, PrintTemplateItem item, long shopId)
        {
            var shop = ServiceContainer.GetService<ShopService>().GetById(shopId);
            if (shop == null || string.IsNullOrWhiteSpace(shop.Mark))
            {
                return "";
            }
            string file = System.IO.Path.Combine(EnvironmentDirHelper.DIR_DATA, shop.Mark + ".png");
            if (File.Exists(file) == false)
            {
                file = System.IO.Path.Combine(EnvironmentDirHelper.DIR_DATA, shop.Mark + ".jpg");
            }

            if (File.Exists(file) == false)
            {
                return null;
            }
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(file);
            bi.EndInit();
            return bi;

        }
    }
}
