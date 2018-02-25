using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ShopErp.App.Converters;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Views.Goods
{
    class GoodUpdateViewModel : DependencyObject
    {
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(string), typeof(GoodUpdateViewModel), new PropertyMetadata(""));

        public ShopErp.Domain.Goods Source { get; set; }

        public string State
        {
            get { return (string) this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }


        private string GetShortString(GoodsState state)
        {
            if (state == GoodsState.NONE)
            {
                return "";
            }
            if (state == GoodsState.WAITPROCESSIMAGE)
            {
                return "";
            }
            if (state == GoodsState.WAITREVIEW)
            {
                return "(图)";
            }
            if (state == GoodsState.UPLOADED)
            {
                return "(上)";
            }
            if (state == GoodsState.NOTSALE)
            {
                return "(下)";
            }
            return "(ERR)";
        }

        public string TargetShops
        {
            get
            {
                if (this.Source == null || this.Source.Shops == null || this.Source.Shops.Count < 1)
                {
                    return "";
                }
                var ss = this.Source.Shops.Select(obj => ShopMarkConverter.Convert(obj.ShopId) + GetShortString(obj.State));
                return string.Join(",", ss);
            }
        }
    }
}