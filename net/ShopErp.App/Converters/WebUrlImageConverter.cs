using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using ShopErp.App.Service;
using ShopErp.App.Service.Net;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Converters
{
    public class WebUrlImageConverter : IValueConverter
    {
        static string IMAGE_DIR = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR);
        private String IMAGE_MODE = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_MODE);

        public static readonly BitmapSource DISABLEIMAGE = new BitmapImage(new Uri("/ShopErp.App;component/Resources/Images/DISABLEIMAGE.jpg", UriKind.Relative));
        public static readonly BitmapSource EMPTYIMAGE = new BitmapImage(new Uri("/ShopErp.App;component/Resources/Images/EMPTYIMAGE.jpg", UriKind.Relative));
        public static readonly BitmapSource READIMAGEERROR = new BitmapImage(new Uri("/ShopErp.App;component/Resources/Images/READIMAGEERROR.jpg", UriKind.Relative));

        static WebUrlImageConverter()
        {
            if (IMAGE_DIR.EndsWith("\\") == false)
            {
                IMAGE_DIR += "\\";
            }
        }

        public static object ConvertLocal(string value)
        {
            string path = value;
            if (("abcdefghijk".Any(c => char.ToLower(value[0]) == c) && value[1] == ':') == false && value.StartsWith("ftp:", StringComparison.OrdinalIgnoreCase) == false)
            {
                path = IMAGE_DIR + value;
            }
            if (File.Exists(path) == false)
            {
                return null;
            }
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new MemoryStream(File.ReadAllBytes(path));
            image.EndInit();
            return image;
        }

        public static object ConvertWeb(string value)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para["image"] = value;
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new MemoryStream(MsHttpRestful.GetUrlEncodeBodyReturnBytes(ServiceContainer.ServerAddress + "//image/getimage.html", para));
            image.EndInit();
            return image;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ("不显示" == IMAGE_MODE)
            {
                return DISABLEIMAGE;
            }

            string img = value as string;
            if (string.IsNullOrWhiteSpace(img))
            {
                return EMPTYIMAGE;
            }

            try
            {
                if (img.StartsWith("//"))
                {
                    img = "http:" + img;
                }

                if (img.StartsWith("http"))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = new MemoryStream(MsHttpRestful.DoWithRetry(() => MsHttpRestful.GetUrlEncodeBodyReturnBytes(img, null)));
                    image.EndInit();
                    return image;
                }

                if (string.IsNullOrWhiteSpace(IMAGE_MODE) || IMAGE_MODE == "内网")
                {
                    return ConvertLocal(img);
                }
                else if (IMAGE_MODE == "外网")
                {
                    return ConvertWeb(img);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return READIMAGEERROR;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}