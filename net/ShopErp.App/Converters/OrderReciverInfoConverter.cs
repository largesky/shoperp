using System;
using System.Windows.Data;
using ShopErp.Domain;

namespace ShopErp.App.Converters
{
    public class OrderReceiverInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Order or = value as Order;
            if (or == null)
            {
                return "";
            }

            return or.ReceiverName + " " + or.ReceiverPhone + " " + or.ReceiverMobile + " " + or.ReceiverAddress;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}