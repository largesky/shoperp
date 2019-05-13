using ShopErp.App.Domain;
using ShopErp.App.Service.Print;
using ShopErp.App.Views.Print;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ShopErp.App.ViewModels
{
    class PrintTemplateItemViewModelForReciverInfoAll : PrintTemplateItemViewModelCommon
    {
        public PrintTemplateItemViewModelForReciverInfoAll(PrintTemplate template) : base(template)
        {
            this.PropertyUI = new PrintTemplateItemReciverPhoneUserControl();
            this.PropertyUI.DataContext = this;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == PrintTemplateItemViewModelCommon.FormatProperty)
            {
                string info = e.NewValue.ToString() == "否" ? "张三  12345678900" : "张三  123****8900";
                this.PreviewValue = info + Environment.NewLine + "四川省 成都市  天府新区 天府三街18号";
            }
        }
    }
}
