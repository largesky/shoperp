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
            var wt = value as PrintTemplate;
            if (wt == null)
            {
                return "";
            }
            return string.Format("{0}-{1}", wt.SourceType == PrintTemplateSourceType.SELF ? "自研" : (wt.IsIsv ? "ISV" : "卖家"), wt.Name);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
