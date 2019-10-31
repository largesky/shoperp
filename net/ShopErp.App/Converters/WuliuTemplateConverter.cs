using ShopErp.App.Service.Restful;
using ShopErp.App.Utils;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ShopErp.App.Converters
{
    public class WuliuTemplateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var wt = value as WuliuPrintTemplate;
            if (wt == null)
            {
                return "";
            }
            return wt.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
