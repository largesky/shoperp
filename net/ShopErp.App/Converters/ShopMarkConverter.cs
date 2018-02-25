using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Converters
{
    public class ShopMarkConverter : IValueConverter
    {
        static List<Shop> shops = new List<Shop>();

        public static string Convert(long shopId)
        {
            if (shops.Count < 1)
            {
                lock (shops)
                {
                    if (shops.Count < 1)
                    {
                        shops.AddRange(ServiceContainer.GetService<ShopService>().GetByAll().Datas);
                    }
                }
            }
            var shop = shops.FirstOrDefault(obj => obj.Id == shopId);
            return shop == null ? "不存在" : shop.Mark;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert((long)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}