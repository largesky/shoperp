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
    public class VideoTypeConverter : IValueConverter
    {
        public static readonly BitmapSource VideoIcon = new BitmapImage(new Uri("/ShopErp.App;component/Resources/Images/VideoIcon.png", UriKind.Relative));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GoodsVideoType vt = (GoodsVideoType)value;
            if (vt == GoodsVideoType.VIDEO)
            {
                return VideoIcon;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
