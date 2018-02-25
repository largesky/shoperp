using System;
using System.Windows.Data;
using ShopErp.App.Utils;
using ShopErp.Domain;
using System.Windows.Media.Imaging;

namespace ShopErp.App.Converters
{
    public class ColorFlagImageConverter : IValueConverter
    {
        public static readonly BitmapSource IMAGE_RED = new BitmapImage(new Uri("/ShopErp.App;component/Resources/Images/Red.png", UriKind.Relative));

        public static readonly BitmapSource IMAGE_GREEN = new BitmapImage(new Uri("/ShopErp.App;component/Resources/Images/Green.png", UriKind.Relative));

        public static readonly BitmapSource IMAGE_YELLOW = new BitmapImage(new Uri("/ShopErp.App;component/Resources/Images/Yellow.png", UriKind.Relative));

        public static readonly BitmapSource IMAGE_BLUE = new BitmapImage(new Uri("/ShopErp.App;component/Resources/Images/Blue.png", UriKind.Relative));

        public static readonly BitmapSource IMAGE_PINK = new BitmapImage(new Uri("/ShopErp.App;component/Resources/Images/Pink.png", UriKind.Relative));

        public static readonly BitmapSource IMAGE_UN_LABLE = new BitmapImage(new Uri("/ShopErp.App;component/Resources/Images/UnLable.png", UriKind.Relative));

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ColorFlag flag = (ColorFlag)value;
            if (flag == ColorFlag.UN_LABEL)
            {
                return IMAGE_UN_LABLE;
            }

            if (flag == ColorFlag.BLUE)
            {
                return IMAGE_BLUE;
            }

            if (flag == ColorFlag.GREEN)
            {
                return IMAGE_GREEN;
            }

            if (flag == ColorFlag.PINK)
            {
                return IMAGE_PINK;
            }

            if (flag == ColorFlag.RED)
            {
                return IMAGE_RED;
            }

            if (flag == ColorFlag.YELLOW)
            {
                return IMAGE_YELLOW;
            }

            return IMAGE_UN_LABLE;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}