using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ShopErp.App.Converters
{
    class PopImageConverter : IValueConverter
    {

        public static readonly BitmapSource IMAGE_TB = new BitmapImage(new Uri("/ShopErp.App;component/Resources/Images/Pop_Tb.png", UriKind.Relative));

        public static readonly BitmapSource IMAGE_PDD = new BitmapImage(new Uri("/ShopErp.App;component/Resources/Images/Pop_Pdd.png", UriKind.Relative));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            PopType pt = (PopType)value;

            if (pt == PopType.TAOBAO || pt == PopType.TMALL || pt == PopType.ALIBABA)
            {
                return IMAGE_TB;
            }

            if (pt == PopType.PINGDUODUO)
            {
                return IMAGE_PDD;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
