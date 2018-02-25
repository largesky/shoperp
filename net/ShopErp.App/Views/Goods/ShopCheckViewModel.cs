 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ShopErp.Domain;

namespace ShopErp.App.Views.Goods
{
    class ShopCheckViewModel : DependencyObject
    {
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(ShopCheckViewModel));

        public Shop Source { get; set; }

        public bool IsChecked
        {
            get { return (bool) this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        public ShopCheckViewModel(Shop source)
        {
            this.Source = source;
        }
    }
}