using System;
using System.Windows;
using System.Windows.Data;
using ShopErp.Domain;

namespace ShopErp.App.Converters
{
    public class OrderPayTypeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PopPayType type = (PopPayType) value;

            if (type == PopPayType.COD)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}