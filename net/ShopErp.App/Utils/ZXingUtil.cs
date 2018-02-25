using System;
using System.Windows;
using System.Windows.Media.Imaging;
using ZXing;

namespace ShopErp.App.Utils
{
    public class ZXingUtil
    {
        public static BitmapSource CreateImage(string content, string format, int width, int height, bool pureBarcode, string fontName, float fontSize)
        {
            var writer = new ZXing.BarcodeWriter();
            writer.Format = (BarcodeFormat)(Enum.Parse(typeof(BarcodeFormat), format));
            writer.Options = new ZXing.Common.EncodingOptions
            {
                Height = height,
                Width = width,
                Margin = 0,
                PureBarcode = pureBarcode
            };
            if (writer.Renderer is ZXing.Rendering.BitmapRenderer)
            {
                ((ZXing.Rendering.BitmapRenderer)writer.Renderer).TextFont = new System.Drawing.Font(fontName, fontSize <= 0 ? 12 : fontSize);
            }
            else if (writer.Renderer is ZXing.Rendering.WriteableBitmapRenderer)
            {
                ((ZXing.Rendering.WriteableBitmapRenderer)writer.Renderer).FontFamily = new System.Windows.Media.FontFamily(fontName); ((ZXing.Rendering.WriteableBitmapRenderer)writer.Renderer).FontSize = fontSize <= 0 ? 12 : fontSize;
            }
            var imageData = writer.Write(content);
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(imageData.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight((int)imageData.Width, (int)imageData.Height));
        }
    }
}