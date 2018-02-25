using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ShopErp.Domain;
using ShopErp.Domain.Pop;

namespace ShopErp.App.ViewModels
{
    class OrderCodViewModel : DependencyObject
    {
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(string), typeof(OrderCodViewModel));

        public static readonly DependencyProperty DeliveryInfoProperty =
            DependencyProperty.Register("DeliveryInfo", typeof(PopDeliveryInfo), typeof(OrderCodViewModel));

        public string State
        {
            get { return (string) this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }

        public Order Source { get; set; }

        public PopDeliveryInfo DeliveryInfo
        {
            get { return (PopDeliveryInfo) this.GetValue(DeliveryInfoProperty); }
            set { this.SetValue(DeliveryInfoProperty, value); }
        }

        public OrderCodViewModel(Order os)
        {
            this.Source = os;
        }
    }
}