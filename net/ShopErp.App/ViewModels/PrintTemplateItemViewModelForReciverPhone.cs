using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ShopErp.App.Domain;
using ShopErp.App.Service.Print;
using ShopErp.App.Views.Print;
using ShopErp.Domain;

namespace ShopErp.App.ViewModels
{
    class PrintTemplateItemViewModelForReciverPhone : PrintTemplateItemViewModelCommon
    {
        public PrintTemplateItemViewModelForReciverPhone(PrintTemplate template) : base(template)
        {
            this.PropertyUI = new PrintTemplateItemReciverPhoneUserControl();
            this.PropertyUI.DataContext = this;
            this.PreviewValue = "158****1234";
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == PrintTemplateItemViewModelCommon.FormatProperty)
            {
                this.PreviewValue = e.NewValue.ToString()== "否" ? "12345678900" : "123****8900";
            }
        }
    }
}
