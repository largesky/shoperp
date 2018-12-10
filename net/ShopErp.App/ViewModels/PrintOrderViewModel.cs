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

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(string), typeof(PrintOrderViewModel));

        public static readonly DependencyProperty DeliveryCompanyProperty = DependencyProperty.Register("DeliveryCompany", typeof(string), typeof(PrintOrderViewModel));

        public static readonly DependencyProperty DeliveryNumberProperty = DependencyProperty.Register("DeliveryNumber", typeof(string), typeof(PrintOrderViewModel));

        public static readonly DependencyProperty ReceiverNameProperty = DependencyProperty.Register("ReceiverName", typeof(string), typeof(PrintOrderViewModel));

        public static readonly DependencyProperty ReceiverPhoneProperty = DependencyProperty.Register("ReceiverPhone", typeof(string), typeof(PrintOrderViewModel));

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

        public string State
        {
            get { return (string)this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }

        public string DeliveryCompany
        {
            get { return (string)this.GetValue(DeliveryCompanyProperty); }
            set { this.SetValue(DeliveryCompanyProperty, value); }
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

        public string ReceiverPhone
        {
            get { return (string)this.GetValue(ReceiverPhoneProperty); }
            set { this.SetValue(ReceiverPhoneProperty, value); }
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

        public string Action
        {
            get { return "删除"; }
        }

        public string Goods { get; private set; }

        public string DoorNumber { get; private set; }

        public bool AddressParseOk { get; set; }

        public int PageNumber { get; set; }

        private void UpdateGoodsInfo()
        {
            StringBuilder sb = new StringBuilder();
            if (this.Source.OrderGoodss != null)
                foreach (var goods in this.Source.OrderGoodss.Where(obj => obj.State != OrderState.CLOSED && obj.State != OrderState.CANCLED && obj.State != OrderState.SPILTED))
                {
                    sb.Append(VendorService.FormatVendorName(goods.Vendor) + " " + goods.Number + " " + goods.Edtion + " " + goods.Color + " " + goods.Size + " " + goods.Count + ", ");
                }

            this.Goods = sb.ToString();
        }

        private void UpdateDoorNumber()
        {
            string sb = "";
            if (this.Source.OrderGoodss != null)
            {
                foreach (var goods in this.Source.OrderGoodss.Where(obj => obj.State != OrderState.CLOSED && obj.State != OrderState.CANCLED && obj.State != OrderState.SPILTED))
                {
                    string door = VendorService.FormatVendorDoor(ServiceContainer.GetService<VendorService>().GetVendorAddress_InCach(goods.Vendor));
                    if (sb.Contains(door) == false)
                    {
                        sb += door + " ";
                    }
                }
            }

            this.DoorNumber = sb;
        }


        public PrintOrderViewModel(Order order, Brush defaultBackground)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            this.Source = order;
            this.DefaultBackground = defaultBackground;
            this.Background = DefaultBackground;
            this.State = "";
            this.DeliveryCompany = order.DeliveryCompany;
            this.DeliveryNumber = order.DeliveryNumber;
            this.ReceiverPhone = order.ReceiverPhone;
            this.ReceiverMobile = order.ReceiverMobile;
            this.ReceiverName = order.ReceiverName;
            this.ReceiverAddress = order.ReceiverAddress;
            this.IsChecked = true;
            this.UpdateDoorNumber();
            this.UpdateGoodsInfo();
        }

        public int CompareTo(PrintOrderViewModel other)
        {
            if (other == null)
            {
                return 1;
            }

            if (this.DoorNumber.Equals(other.DoorNumber, StringComparison.OrdinalIgnoreCase) == false)
            {
                return this.DoorNumber.CompareTo(other.DoorNumber);
            }

            if (this.Goods.Equals(other.Goods, StringComparison.OrdinalIgnoreCase) == false)
            {
                return this.Goods.CompareTo(other.Goods);
            }
            return this.Source.PopPayTime.CompareTo(other.Source.PopPayTime);
        }
    }
}