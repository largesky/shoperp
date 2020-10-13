using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using ShopErp.App.Converters;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.ViewModels
{
    public class PrintOrderViewModel : DependencyObject, IComparable<PrintOrderViewModel>
    {

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(PrintOrderViewModel));

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(PrintOrderViewModel));

        public static readonly DependencyProperty DeliveryNumberProperty = DependencyProperty.Register("DeliveryNumber", typeof(string), typeof(PrintOrderViewModel));

        public static readonly DependencyProperty ReceiverNameProperty = DependencyProperty.Register("ReceiverName", typeof(string), typeof(PrintOrderViewModel));

        public static readonly DependencyProperty ReceiverMobileProperty = DependencyProperty.Register("ReceiverMobile", typeof(string), typeof(PrintOrderViewModel));

        public static readonly DependencyProperty ReceiverAddressProperty = DependencyProperty.Register("ReceiverAddress", typeof(string), typeof(PrintOrderViewModel));

        public static readonly Brush DEFAULTBACKGROUND_LIGHTPINK = Brushes.LightPink;

        public static readonly Brush DEFAULTBACKGROUND_LIGHTGREEN = Brushes.LightGreen;

        public Brush DefaultBackground { get; set; }

        public Brush Background
        {
            get { return (Brush)this.GetValue(BackgroundProperty); }
            set
            {
                if (value == null) value = DefaultBackground;
                this.SetValue(BackgroundProperty, value);
            }
        }

        public bool IsChecked
        {
            get { return (bool)this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        public string DeliveryNumber
        {
            get { return (string)this.GetValue(DeliveryNumberProperty); }
            set { this.SetValue(DeliveryNumberProperty, value); }
        }

        public string ReceiverName
        {
            get { return (string)this.GetValue(ReceiverNameProperty); }
            set { this.SetValue(ReceiverNameProperty, value); }
        }

        public string ReceiverMobile
        {
            get { return (string)this.GetValue(ReceiverMobileProperty); }
            set { this.SetValue(ReceiverMobileProperty, value); }
        }

        public string ReceiverAddress
        {
            get { return (string)this.GetValue(ReceiverAddressProperty); }
            set { this.SetValue(ReceiverAddressProperty, value); }
        }

        public Order Source { get; private set; }

        public WuliuNumber WuliuNumber { get; set; }

        public string Goods { get; private set; }

        public int PageNumber { get; set; }

        public PrintOrderViewModel(Order order, Brush defaultBackground)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            this.Source = order;
            this.DefaultBackground = defaultBackground;
            this.Background = DefaultBackground;
            this.DeliveryNumber = order.DeliveryNumber;
            this.ReceiverMobile = order.ReceiverMobile;
            this.ReceiverName = order.ReceiverName;
            this.ReceiverAddress = order.ReceiverAddress;
            this.IsChecked = order.Type == OrderType.SHUA ? false : true;
            this.Goods = OrderService.FormatGoodsInfoCanbeSend(order);
        }

        public int CompareTo(PrintOrderViewModel other)
        {
            if (other == null)
            {
                return 1;
            }

            if (this.Goods.Equals(other.Goods, StringComparison.OrdinalIgnoreCase) == false)
            {
                return this.Goods.CompareTo(other.Goods);
            }

            return this.Source.PopPayTime.CompareTo(other.Source.PopPayTime);
        }
    }
}