using System;
using System.Windows.Data;
using ShopErp.App.Utils;
using ShopErp.Domain;

namespace ShopErp.App.Converters
{
    public class OrderGoodsStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            OrderState og = (OrderState) value;
            if ((int) og <= (int) OrderState.PRINTED)
            {
                return "";
            }

            return EnumUtil.GetEnumValueDescription(og);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}