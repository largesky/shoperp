using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ShopErp.App.Domain;
using ShopErp.App.Service.Print;
using ShopErp.App.Service.Print.OtherFormatters;
using ShopErp.Domain;

namespace ShopErp.App.ViewModels
{
    public class PrintTemplateItemViewModelForRandom : PrintTemplateItemViewModelCommon
    {
        private OtherRandom r = new OtherRandom();

        public PrintTemplateItemViewModelForRandom(PrintTemplate printTemplate) : base(printTemplate)
        {
            this.PreviewValue = "HDF-32";
            this.PropertyUI = new Views.Print.PrintTemplateItemRandomUserControl();
            this.PropertyUI.DataContext = this;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == PrintTemplateItemViewModelCommon.FormatProperty ||
                e.Property == PrintTemplateItemViewModelCommon.ValueProperty
                || e.Property == PrintTemplateItemViewModelCommon.Value1Property)
            {
                if (this.Data != null && String.IsNullOrWhiteSpace(this.Data.Format) == false &&
                    string.IsNullOrWhiteSpace(this.Data.Value) == false)
                {
                    var s = this.r.Format(this.Template, this.Data);
                    this.PreviewValue = s;
                }
                else
                {
                    this.PreviewValue = "随机序列";
                }
            }
        }
    }
}