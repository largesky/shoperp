using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ShopErp.Domain;

namespace ShopErp.App.ViewModels
{
    class DeliveryInViewModel : DependencyObject
    {
        public static readonly DependencyProperty IdProperty = DependencyProperty.Register("Id", typeof(long), typeof(DeliveryInViewModel), new PropertyMetadata(0L));

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(string), typeof(DeliveryInViewModel));

        public long Id { get { return (long)this.GetValue(IdProperty); } set { this.SetValue(IdProperty, value); } }

        public long OrderId { get; set; }

        public long ShopId { get; set; }

        public string ReceiverMobile { get; set; }

        public string OrderGoodsInfo { get; set; }

        public string DeliveryCompany { get; set; }

        public string DeliveryNumber { get; set; }

        public string State { get { return (string)this.GetValue(StateProperty); } set { this.SetValue(StateProperty, value); } }

        public string Action { get; set; }

        public Order SourceOrder { get; set; }

        public OrderGoods SourceOrderGoods { get; set; }

        public bool IsRefused { get; set; }
    }
}