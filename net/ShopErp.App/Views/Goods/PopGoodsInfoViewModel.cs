
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ShopErp.Domain;
using ShopErp.Domain.Pop;

namespace ShopErp.App.Views.Goods
{
    public class PopGoodsInfoViewModel : DependencyObject
    {
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(string), typeof(PopGoodsInfoViewModel));
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(PopGoodsInfoViewModel));

        public PopGoods PopGoodsInfo { get; private set; }

        public GoodsTask GoodsTask { get; set; }

        public int UserNumber { get; set; }

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        public string State
        {
            get { return (string)this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }

        public string SkuCodesInfo
        {
            get
            {
                return string.Join(",", this.PopGoodsInfo.Skus.Where(obj => string.IsNullOrWhiteSpace(obj.Code) == false).Select(obj => obj.Code.Trim()).Distinct());
            }
        }

        public string EditStr
        {
            get { return "前往编辑"; }
        }

        public PopGoodsInfoViewModel(PopGoods popGoodsInfo)
        {
            this.PopGoodsInfo = popGoodsInfo;
        }
    }
}