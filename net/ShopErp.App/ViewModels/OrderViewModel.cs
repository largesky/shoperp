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
    public class OrderViewModel : DependencyObject
    {
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(OrderViewModel));

        public static readonly DependencyProperty OrderFlagProperty = DependencyProperty.Register("OrderFlag", typeof(ColorFlag), typeof(OrderViewModel));

        public static readonly DependencyProperty PopSellerCommentProperty = DependencyProperty.Register("PopSellerComment", typeof(string), typeof(OrderViewModel));

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(OrderViewModel));

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(string), typeof(OrderViewModel));

        public static readonly DependencyProperty DeliveryCompanyProperty = DependencyProperty.Register("DeliveryCompany", typeof(string), typeof(OrderViewModel));

        public static readonly DependencyProperty DeliveryNumberProperty = DependencyProperty.Register("DeliveryNumber", typeof(string), typeof(OrderViewModel));


        public bool IsChecked
        {
            get { return (bool)this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        public ColorFlag OrderFlag
        {
            get { return (ColorFlag)this.GetValue(OrderFlagProperty); }
            set { this.SetValue(OrderFlagProperty, value); }
        }

        public string PopSellerComment
        {
            get { return (string)this.GetValue(PopSellerCommentProperty); }
            set { this.SetValue(PopSellerCommentProperty, value); }
        }

        public Brush Background
        {
            get { return (Brush)this.GetValue(BackgroundProperty); }
            set { this.SetValue(BackgroundProperty, value); }
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

        public string State
        {
            get { return (string)this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }

        public string GoodsInfo
        {
            get
            {
                if (this.Source.OrderGoodss == null || this.Source.OrderGoodss.Count < 1)
                {
                    return "";
                }

                StringBuilder sb = new StringBuilder();
                if (this.Source.OrderGoodss != null && this.Source.OrderGoodss.Count > 0)
                {
                    foreach (var goods in this.Source.OrderGoodss)
                    {
                        sb.Append(VendorService.FormatVendorName(goods.Vendor) + " " + goods.Number + goods.Edtion + goods.Color + goods.Size + " (" + goods.Count + ") ");
                    }
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 可以发货的商品信息
        /// </summary>
        public string GoodsInfoCanbeCount
        {
            get
            {
                if (this.Source.OrderGoodss == null || this.Source.OrderGoodss.Count < 1)
                {
                    return "";
                }

                StringBuilder sb = new StringBuilder();
                if (this.Source.OrderGoodss != null && this.Source.OrderGoodss.Count > 0)
                {
                    foreach (var goods in this.Source.OrderGoodss)
                    {
                        if ((int)goods.State >= (int)OrderState.PAYED && (int)goods.State < (int)OrderState.SHIPPED)
                            sb.Append(VendorService.FormatVendorName(goods.Vendor) + " " + goods.Number + goods.Edtion + goods.Color + goods.Size + " (" + goods.Count + ") ");
                    }
                }
                return sb.ToString();
            }
        }

        public string Phone
        {
            get
            {
                return string.Join(" ", Source.ReceiverMobile, Source.ReceiverPhone).Trim();
            }
        }

        public string Expander
        {
            get
            {
                return this.HistoryOrders != null && this.HistoryOrders.Count > 0 ? "史" : "";
            }
        }

        public Order Source { get; private set; }

        public List<OrderViewModel> HistoryOrders { get; private set; }

        public OrderViewModel(Order order)
        {
            this.Source = order;
            this.OrderFlag = order.PopFlag;
            this.PopSellerComment = order.PopSellerComment;
            this.HistoryOrders = new List<OrderViewModel>();
            this.DeliveryCompany = order.DeliveryCompany;
            this.DeliveryNumber = order.DeliveryNumber;
        }
    }
}