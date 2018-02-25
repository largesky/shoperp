using System;
using System.Linq;
using System.Windows.Data;

namespace ShopErp.App.Converters
{
    public class BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string p = parameter.ToString();
            string[] ps = p.Split(",|".ToArray());
            if (ps.Length != 2)
            {
                throw new Exception("参数不正确");
            }
            if (value == null)
            {
                return ps[0];
            }

            if (value.GetType() == typeof(bool?))
            {
                return ((bool?) value).Value ? ps[1] : ps[0];
            }
            return ((bool) value) ? ps[1] : ps[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}