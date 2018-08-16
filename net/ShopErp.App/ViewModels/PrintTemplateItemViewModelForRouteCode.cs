using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopErp.App.Domain;
using ShopErp.App.Service.Print;
using ShopErp.App.Views.Print;

namespace ShopErp.App.ViewModels
{
    class PrintTemplateItemViewModelForRouteCode : PrintTemplateItemViewModelCommon
    {
        public PrintTemplateItemViewModelForRouteCode(Service.Print.PrintTemplate template) : base(template)
        {
            this.PropertyUI = new PrintTemplateItemViewModelRouteCodeUserControl();
            this.PropertyUI.DataContext = this;
        }

        protected override void OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == PrintTemplateItemViewModelCommon.FormatProperty)
            {
                string value = e.NewValue.ToString();
                string pri = "";

                if (value == "全部")
                {
                    pri = "650-480 220";
                }
                else if (value == "第一段")
                {
                    pri = "650";
                }
                else if (value == "第二段")
                {
                    pri = "480";
                }
                else
                {
                    pri = "220";
                }
                this.PreviewValue = pri;
            }
            base.OnPropertyChanged(e);
        }
    }
}
