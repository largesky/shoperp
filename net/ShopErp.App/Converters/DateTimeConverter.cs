using System;
using System.Windows.Data;

namespace ShopErp.App.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        private static DateTime MIN_TIME = new DateTime(2000, 01, 01);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            DateTime dt = Utils.DateTimeUtil.DbMinTime;

            if (value.GetType() == typeof(DateTime?))
            {
                dt = ((DateTime?) value).Value;
            }
            else
            {
                dt = (DateTime) value;
            }

            if (dt <= MIN_TIME)
            {
                return "";
            }

            if (parameter == null || parameter.GetType() != typeof(string))
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            return parameter + dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}