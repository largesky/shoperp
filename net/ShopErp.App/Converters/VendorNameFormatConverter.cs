using System;
using System.Windows.Data;
using ShopErp.App.Service.Restful;

namespace ShopErp.App.Converters
{
    public class VendorNameFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var v = (long)value;
            if (v == 0)
            {
                return "";
            }

            return VendorService.FormatVendorName(ServiceContainer.GetService<VendorService>().GetVendorName(v));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}