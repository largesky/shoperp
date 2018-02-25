using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.ViewModels
{
    public class DeliveryCheckViewModel : DependencyObject
    {
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(DeliveryCheckViewModel));

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(DeliveryCheckViewModel));

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(string), typeof(DeliveryCheckViewModel));

        public static readonly DependencyProperty FirstDeliveryInfoProperty =
            DependencyProperty.Register("FirstDeliveryInfo", typeof(string), typeof(DeliveryCheckViewModel));

        public static readonly DependencyProperty SecondDeliveryInfoProperty =
            DependencyProperty.Register("SecondDeliveryInfo", typeof(string), typeof(DeliveryCheckViewModel));

        public Order Source { get; set; }

        public string GoodsInfo
        {
            get
            {
                if (this.Source == null)
                {
                    return "";
                }

                if (this.Source.OrderGoodss == null || this.Source.OrderGoodss.Count < 1)
                {
                    return "";
                }

                StringBuilder sb = new StringBuilder();
                var vs = ServiceContainer.GetService<VendorService>();
                if (this.Source.OrderGoodss != null && this.Source.OrderGoodss.Count > 0)
                {
                    foreach (var goods in this.Source.OrderGoodss.Where(
                        obj => (int) obj.State <= (int) OrderState.SHIPPED))
                    {
                        sb.Append(VendorService.FormatVendorName(goods.Vendor) + " " + goods.Number + goods.Edtion +
                                  goods.Color + goods.Size + " (" + goods.Count + ") ");
                    }
                }
                return sb.ToString();
            }
        }

        public Brush Background
        {
            get { return (Brush) this.GetValue(BackgroundProperty); }
            set { this.SetValue(BackgroundProperty, value); }
        }

        public string State
        {
            get { return (string) this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }

        public string FirstDeliveryInfo
        {
            get { return (string) GetValue(FirstDeliveryInfoProperty); }
            set { this.SetValue(FirstDeliveryInfoProperty, value); }
        }

        public string SecondDeliveryInfo
        {
            get { return (string) GetValue(SecondDeliveryInfoProperty); }
            set { this.SetValue(SecondDeliveryInfoProperty, value); }
        }

        public bool IsChecked
        {
            get { return (bool) this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }


        public DeliveryCheckViewModel(Order order)
        {
            this.Source = order;
        }
    }
}