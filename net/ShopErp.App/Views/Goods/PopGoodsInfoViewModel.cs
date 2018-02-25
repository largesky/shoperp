
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
        public static readonly DependencyProperty StateProperty =DependencyProperty.Register("State", typeof(string), typeof(PopGoodsInfoViewModel));

        public PopGoods PopGoodsInfo { get; private set; }

        public GoodsTask GoodsTask { get; set; }

        public int UserNumber { get; set; }

        public string State
        {
            get { return (string) this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }

        public string EditStr
        {
            get { return "前往编辑"; }
        }

        public bool IsDup { get; set; }

        public string[] SkuCodes { get; set; }

        public string SkuCodesInfo
        {
            get
            {
                if (this.SkuCodes == null)
                {
                    return "";
                }
                return string.Join(",", SkuCodes);
            }
        }

        public string SkuInfo
        {
            get
            {
                if (this.PopGoodsInfo == null || this.PopGoodsInfo.Skus == null || this.PopGoodsInfo.Skus.Count < 1)
                {
                    return "";
                }
                return string.Join(",", this.PopGoodsInfo.Skus.Select(obj => obj.Value));
            }
        }

        public PopGoodsInfoViewModel(PopGoods popGoodsInfo)
        {
            this.PopGoodsInfo = popGoodsInfo;
            this.SkuCodes = popGoodsInfo.Skus.Select(obj => obj.Stock).ToArray();
        }
    }
}