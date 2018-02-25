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
    public class PrintTemplateItemViewModelForDeliveryNumberBarcode : PrintTemplateItemViewModelForBarcode
    {
        public PrintTemplateItemViewModelForDeliveryNumberBarcode(PrintTemplate template) :
            base(template)
        {
            this.PropertyUI = new PrintTemplateItemDeliveryNumberBarcodeUserControl();
            this.Value = "D00099991111";
            this.Value1 = "否";
        }
    }
}