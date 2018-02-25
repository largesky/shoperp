using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ShopErp.App.Utils;
using ShopErp.Domain;

namespace ShopErp.App.ViewModels
{
    public class OrderFlagViewModel : DependencyObject
    {
        public static readonly DependencyProperty IsCheckedProperty =DependencyProperty.Register("IsChecked", typeof(bool), typeof(OrderFlagViewModel));

        public static readonly DependencyProperty FlagProerty =DependencyProperty.Register("Flag", typeof(ColorFlag), typeof(OrderFlagViewModel));

        public string Color { get; set; }

        public bool IsChecked
        {
            get { return (bool) this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        public ColorFlag Flag
        {
            get { return (ColorFlag) this.GetValue(FlagProerty); }
            set { this.SetValue(FlagProerty, value); }
        }

        public OrderFlagViewModel(bool isChecked, ColorFlag flag)
        {
            this.IsChecked = isChecked;
            this.Flag = flag;
            this.Color = EnumUtil.GetEnumValueDescription(flag).Trim('色');
        }
    }
}