﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using ShopErp.Domain;
using ShopErp.Domain.Pop;

namespace ShopErp.App.Views.Goods
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PopGoodsInfoViewModel : DependencyObject
    {
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(string), typeof(PopGoodsInfoViewModel));
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(PopGoodsInfoViewModel));

        [JsonProperty]
        public PopGoods PopGoodsInfo { get; private set; }
        [JsonProperty]
        public GoodsTask GoodsTask { get; set; }
        [JsonProperty]
        public int UserNumber { get; set; }
        [JsonProperty]
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }
        [JsonProperty]
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

        public int TitleLength
        {
            get { return this.PopGoodsInfo.Title.Select(c => char.IsLower(c) || char.IsUpper(c) || char.IsDigit(c) ? 1 : 2).Sum(); }
        }

        public string Liuxinyuanshu
        {
            get
            {
                if (this.PopGoodsInfo.Properties == null)
                {
                    return "";
                }
                var item = this.PopGoodsInfo.Properties.FirstOrDefault(obj => obj.Key == "流行元素");
                if (item == null)
                {
                    return "";
                }
                return item.Value;
            }
        }

        public string EditStr
        {
            get { return "前往编辑"; }
        }

        public PopGoodsInfoViewModel()
        {

        }

        public PopGoodsInfoViewModel(PopGoods popGoodsInfo)
        {
            this.PopGoodsInfo = popGoodsInfo;
        }
    }
}