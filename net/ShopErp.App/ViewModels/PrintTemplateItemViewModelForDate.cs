using ShopErp.App.Domain;
using ShopErp.App.Service.Print;
using ShopErp.App.Views.Print;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ShopErp.App.ViewModels
{
    class PrintTemplateItemViewModelForDate : PrintTemplateItemViewModelCommon
    {
        public PrintTemplateItemViewModelForDate(PrintTemplate template) :
            base(template)
        {
            this.PropertyUI = new PrintTemplateItemDateUserControl();
            this.PropertyUI.DataContext = this;
            this.Format = "yyyy MM dd";
        }

        protected override void OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == PrintTemplateItemViewModelCommon.FormatProperty)
            {
                try
                {
                    this.PreviewValue = DateTime.Now.ToString(e.NewValue.ToString());
                }
                catch
                {
                    MessageBox.Show("请输入合法的时间格式");
                }
            }

            base.OnPropertyChanged(e);
        }
    }
}