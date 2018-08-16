using ShopErp.App.Domain;
using ShopErp.App.Service.Print;
using ShopErp.App.Views.Print;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ShopErp.App.ViewModels
{
    class PrintTemplateItemViewModelForText : PrintTemplateItemViewModelCommon
    {
        public PrintTemplateItemViewModelForText(Service.Print.PrintTemplate template) :
            base(template)
        {
            this.PropertyUI = new PrintTemplateItemTextUserControl();
            this.PropertyUI.DataContext = this;
            this.Format = "自定义文本";
        }

        protected override void OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == PrintTemplateItemViewModelCommon.FormatProperty)
            {
                this.PreviewValue = e.NewValue.ToString();
            }
            base.OnPropertyChanged(e);
        }
    }
}