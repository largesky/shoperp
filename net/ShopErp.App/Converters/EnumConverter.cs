using ShopErp.App.Utils;
using System;
using System.Windows.Data;

namespace ShopErp.App.Converters
{
    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }

            Enum en = (Enum)value;

            return Utils.EnumUtil.GetEnumValueDescription(en);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var e = EnumUtil.GetEnumValueByDesc(targetType, value.ToString());
            return e;
        }
    }
}