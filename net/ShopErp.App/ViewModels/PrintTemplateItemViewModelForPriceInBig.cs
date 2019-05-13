using ShopErp.App.Domain;
using ShopErp.App.Service.Print;
using ShopErp.App.Views.Print;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShopErp.App.ViewModels
{
    class PrintTemplateItemViewModelForPriceInBig : PrintTemplateItemViewModelCommon
    {
        public PrintTemplateItemViewModelForPriceInBig(PrintTemplate template) :
            base(template)
        {
            this.Format = "万 千 百 十 个";
            this.PropertyUI = new PrintTemplateItemPriceInBigUserControl {DataContext = this};
        }

        protected override void OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == PrintTemplateItemViewModelCommon.FormatProperty)
            {
                string strMoney = e.NewValue.ToString();

                if (strMoney.Contains("万"))
                {
                    strMoney = strMoney.Replace('万', '玖');
                }

                if (strMoney.Contains("千"))
                {
                    strMoney = strMoney.Replace('千', '玖');
                }

                if (strMoney.Contains("百"))
                {
                    strMoney = strMoney.Replace('百', '玖');
                }

                if (strMoney.Contains("十"))
                {
                    strMoney = strMoney.Replace('十', '玖');
                }

                if (strMoney.Contains("个"))
                {
                    strMoney = strMoney.Replace('个', '玖');
                }

                this.PreviewValue = strMoney;
            }
            base.OnPropertyChanged(e);
        }
    }
}