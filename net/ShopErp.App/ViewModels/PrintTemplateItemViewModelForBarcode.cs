using ShopErp.App.Domain;
using ShopErp.App.Views.Print;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Rendering;

namespace ShopErp.App.ViewModels
{
    public class PrintTemplateItemViewModelForBarcode : PrintTemplateItemViewModelCommon
    {
        public string[] Types { get; set; }

        public PrintTemplateItemViewModelForBarcode(PrintTemplate template) :
            base(template)
        {
            this.Format = BarcodeFormat.CODE_128.ToString();
            this.Types = MultiFormatWriter.SupportedWriters.Select(obj => obj.ToString()).ToArray();
            this.PropertyUI = new PrintTemplateItemBarcodeUserControl();
            this.PropertyUI.DataContext = this;
            this.PreviewValue = new Image();
            this.Value = "699999999988";
            this.Value1 = "是";
        }

        protected override void OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == PrintTemplateItemViewModelCommon.FormatProperty ||
                e.Property == PrintTemplateItemViewModelCommon.FontSizeProperty ||
                e.Property == PrintTemplateItemViewModelCommon.FontNameProperty ||
                e.Property == PrintTemplateItemViewModelCommon.WidthProperty ||
                e.Property == PrintTemplateItemViewModelCommon.HeightProperty ||
                e.Property == PrintTemplateItemViewModelCommon.ValueProperty ||
                e.Property == PrintTemplateItemViewModelCommon.Value1Property)
            {
                this.GenImage();
            }
            else if (e.Property == PrintTemplateItemViewModelForBarcode.ValueProperty)
            {
                this.GenImage();
                return;
            }
            base.OnPropertyChanged(e);
        }

        private void GenImage()
        {
            if (string.IsNullOrWhiteSpace(Value) || string.IsNullOrWhiteSpace(Format))
            {
                return;
            }
            var writer = new BarcodeWriter();
            writer.Format = (BarcodeFormat) (Enum.Parse(typeof(BarcodeFormat), this.Format));
            writer.Options = new ZXing.Common.EncodingOptions
            {
                Height = (int) this.Height,
                Width = (int) this.Width,
                Margin = 0,
                PureBarcode = this.Value1 == "是" ? false : true,
            };
            if (writer.Renderer is BitmapRenderer)
            {
                ((BitmapRenderer) writer.Renderer).TextFont = new System.Drawing.Font(this.FontName,
                    (float) this.FontSize <= 0 ? 12 : (float) this.FontSize);
            }
            else if (writer.Renderer is WriteableBitmapRenderer)
            {
                ((WriteableBitmapRenderer) writer.Renderer).FontFamily =
                    new System.Windows.Media.FontFamily(this.FontName);
                ((WriteableBitmapRenderer) writer.Renderer).FontSize = this.FontSize <= 0 ? 12 : this.FontSize;
            }
            try
            {
                var imageData = writer.Write(this.Value);
                if (this.PreviewValue is Image == false)
                    this.PreviewValue = new Image();
                var image = this.PreviewValue as Image;
                image.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(imageData.GetHbitmap(),
                    IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight((int) imageData.Width, (int) imageData.Height));
            }
            catch (Exception ex)
            {
                this.PreviewValue = "生成预览出错:" + ex.Message;
            }
        }
    }
}