using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ShopErp.App.ViewModels
{
    public class OrderSpilteViewModel : DependencyObject
    {
        public static readonly DependencyProperty OrderIdProperty = DependencyProperty.Register("OrderId", typeof(long), typeof(OrderSpilteViewModel));

        public static readonly DependencyProperty OrderGoodsIdProperty = DependencyProperty.Register("OrderGoodsId", typeof(long), typeof(OrderSpilteViewModel));

        public static readonly DependencyProperty OrderGoodsInfoProperty = DependencyProperty.Register("OrderGoodsInfo", typeof(string), typeof(OrderSpilteViewModel));

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register("Count", typeof(int), typeof(OrderSpilteViewModel));

        public static readonly DependencyProperty SpilteCountProperty = DependencyProperty.Register("SpilteCount", typeof(int), typeof(OrderSpilteViewModel));

        public long OrderId
        {
            get { return (long)this.GetValue(OrderIdProperty); }
            set { this.SetValue(OrderIdProperty, value); }
        }

        public long OrderGoodsId
        {
            get { return (long)this.GetValue(OrderGoodsIdProperty); }
            set { this.SetValue(OrderGoodsIdProperty, value); }
        }

        public string OrderGoodsInfo
        {
            get { return (string)this.GetValue(OrderGoodsInfoProperty); }
            set { this.SetValue(OrderGoodsInfoProperty, value); }
        }

        public int Count
        {
            get { return (int)this.GetValue(CountProperty); }
            set { this.SetValue(CountProperty, value); }
        }

        public int SpilteCount
        {
            get { return (int)this.GetValue(SpilteCountProperty); }
            set { this.SetValue(SpilteCountProperty, value); }
        }

        public OrderState State { get; set; }

        public string Comment { get; set; }

        public OrderGoods OrderGoods { get; set; }

        public OrderSpilteViewModel(OrderGoods orderGoods)
        {
            this.OrderGoods = orderGoods;
            this.OrderId = orderGoods.OrderId;
            this.OrderGoodsId = orderGoods.Id;
            this.State = orderGoods.State;
            this.Comment = orderGoods.Comment;
            this.Count = orderGoods.Count;
            this.OrderGoodsInfo = orderGoods.Vendor + " " + orderGoods.Number + " " + orderGoods.Edtion + " " + orderGoods.Color + " " + orderGoods.Size + " " + orderGoods.Count;
        }
    }
}