using System;
using System.Windows;
using System.Windows.Media.Imaging;
using ZXing;

namespace ShopErp.App.Utils
{
    public class ZXingUtil
    {
        public static System.Drawing.Bitmap CreateImage(string content, string format, int width, int height, bool pureBarcode, string fontName, float fontSize)
        {
            var writer = new ZXing.BarcodeWriter();
            writer.Format = (BarcodeFormat)(Enum.Parse(typeof(BarcodeFormat), format));
            writer.Options = new ZXing.Common.EncodingOptions
            {
                Height = height,
                Width = width,
                Margin = 0,
                PureBarcode = pureBarcode,
            };
            if (writer.Renderer is ZXing.Rendering.BitmapRenderer)
            {
                ((ZXing.Rendering.BitmapRenderer)writer.Renderer).TextFont = new System.Drawing.Font(fontName, fontSize <= 0 ? 12 : fontSize);
            }
            else if (writer.Renderer is ZXing.Rendering.WriteableBitmapRenderer)
            {
                ((ZXing.Rendering.WriteableBitmapRenderer)writer.Renderer).FontFamily = new System.Windows.Media.FontFamily(fontName); ((ZXing.Rendering.WriteableBitmapRenderer)writer.Renderer).FontSize = fontSize <= 0 ? 12 : fontSize;
            }
            var data = writer.Encode(content);

            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(data.Width, data.Height);

            for (int x = 0; x < data.Width; x++)
            {
                for (int y = 0; y < data.Height; y++)
                {
                    bitmap.SetPixel(x, y, data[x, y] ? System.Drawing.Color.Black : System.Drawing.Color.White);
                }
            }
            return bitmap;
        }
    }
}