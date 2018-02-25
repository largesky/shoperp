using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ShopErp.Domain;

namespace ShopErp.App.ViewModels
{
    public class OrderReturnViewModel : DependencyObject
    {
        public static readonly DependencyProperty ProcessStateProperty = DependencyProperty.Register("ProcessState", typeof(string), typeof(OrderReturnViewModel), new PropertyMetadata(""));

        public string ProcessState
        {
            get { return (string)this.GetValue(ProcessStateProperty); }
            set { this.SetValue(ProcessStateProperty, value); }
        }

        public OrderReturn Source { get; set; }

        public Order Order { get; set; }

        public OrderGoods OrderGoods { get; set; }

        public string GoodsInfo
        {
            get { return this.Source.GoodsInfo; }
        }

        public double Price
        {
            get { return this.Source.GoodsMoney / this.Source.Count; }
        }

        public int Count
        {
            get { return this.Source.Count; }
        }

        public string Action
        {
            get { return "删除"; }
        }

        public OrderReturnViewModel(OrderReturn or)
        {
            this.Source = or;
        }
    }
}