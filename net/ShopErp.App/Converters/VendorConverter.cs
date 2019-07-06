using System;
using System.Collections.Generic;
using System.Windows.Data;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using System.Linq;

namespace ShopErp.App.Converters
{
    public class VendorConverter : IValueConverter
    {
        private static readonly List<Vendor> vendors = new List<Vendor>();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            var vendor = ServiceContainer.GetService<VendorService>().GetVendorName(long.Parse(value.ToString()));
            return vendor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string name = value.ToString();
            var vendors = ServiceContainer.GetService<VendorService>().GetByAll(name, "", "", "", 0, 0).Datas.Where(obj => obj.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();

            if (vendors.Count == 0)
            {
                return 0;
            }

            if (vendors.Count == 1)
            {
                return vendors.First().Id;
            }

            return -1;

        }
    }
}