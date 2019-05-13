using ShopErp.App.Domain;
using ShopErp.App.Service.Print;
using ShopErp.App.Utils;
using ShopErp.App.Views.Print;
using ShopErp.Domain;
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
            this.Value = "是";
            this.Value1 = "699999999988";
        }

        protected override void OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == PrintTemplateItemViewModelCommon.FontSizeProperty ||
                e.Property == PrintTemplateItemViewModelCommon.FontNameProperty ||
                e.Property == PrintTemplateItemViewModelCommon.WidthProperty ||
                e.Property == PrintTemplateItemViewModelCommon.HeightProperty ||
                e.Property == PrintTemplateItemViewModelCommon.FormatProperty ||
                e.Property == PrintTemplateItemViewModelCommon.ValueProperty ||
                e.Property == PrintTemplateItemViewModelCommon.Value1Property)
            {
                this.GenImage();
            }
            base.OnPropertyChanged(e);
        }

        private void GenImage()
        {
            if (string.IsNullOrWhiteSpace(Value) || string.IsNullOrWhiteSpace(Format))
            {
                return;
            }
            try
            {
                System.Drawing.Bitmap imageData = ZXingUtil.CreateImage(this.Value1, this.Format, (int)this.Width, (int)this.Height, this.Value == "是" ? false : true, this.FontName, (int)this.FontSize);
                var bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(imageData.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight((int)imageData.Width, (int)imageData.Height));

                if (this.PreviewValue is Image == false)
                    this.PreviewValue = new Image();
                var image = this.PreviewValue as Image;
                image.Source = bs;
            }
            catch (Exception ex)
            {
                this.PreviewValue = "生成预览出错:" + ex.Message;
            }
        }
    }
}