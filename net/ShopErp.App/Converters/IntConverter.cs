using System;
using System.Windows.Data;

namespace ShopErp.App.Converters
{
    public class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || (value.GetType() != typeof(int) && value is Enum == false))
            {
                return "";
            }

            string[] para = parameter.ToString().Split('|');
            return para[(int) value];
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}