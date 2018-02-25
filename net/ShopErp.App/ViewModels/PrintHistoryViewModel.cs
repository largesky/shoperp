using ShopErp.App.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ShopErp.App.Utils;
using ShopErp.Domain;
using ShopErp.App.Service.Restful;

namespace ShopErp.App.ViewModels
{
    class PrintHistoryViewModel : DependencyObject
    {
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(PrintHistoryViewModel));

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(string), typeof(PrintHistoryViewModel));

        public static readonly DependencyProperty DeliveryNumberProperty =
            DependencyProperty.Register("DeliveryNumber", typeof(string), typeof(PrintHistoryViewModel));

        public static readonly DependencyProperty DeliveryCompanyProperty =
            DependencyProperty.Register("DeliveryCompany", typeof(string), typeof(PrintHistoryViewModel));

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(PrintHistoryViewModel));

        public static readonly Brush DEFAULTBACKGROUND_LIGHTPINK = Brushes.LightPink;
        public static readonly Brush DEFAULTBACKGROUND_LIGHTGREEN = Brushes.LightGreen;

        public Brush DefaultBackground { get; private set; }

        public Brush Background
        {
            get { return (Brush) this.GetValue(BackgroundProperty); }
            set
            {
                if (value == null) value = DefaultBackground;
                this.SetValue(BackgroundProperty, value);
            }
        }

        public bool IsChecked
        {
            get { return (bool) this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        public string State
        {
            get { return (string) this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }

        public string DeliveryCompany
        {
            get { return (string) this.GetValue(DeliveryCompanyProperty); }
            set { this.SetValue(DeliveryCompanyProperty, value); }
        }

        public string DeliveryNumber
        {
            get { return (string) this.GetValue(DeliveryNumberProperty); }
            set { this.SetValue(DeliveryNumberProperty, value); }
        }

        public PrintHistory Source { get; private set; }

        public string IsUploaded
        {
            get { return this.Source.UploadTime <= new DateTime(2010, 01, 01) ? "" : "是"; }
        }

        public PrintHistoryViewModel(PrintHistory source, Brush defaultBackground)
        {
            this.Source = source;
            this.DeliveryCompany = source.DeliveryCompany;
            this.DeliveryNumber = source.DeliveryNumber;
            this.IsChecked = false;
            this.State = "";
            this.DefaultBackground = defaultBackground;
            this.Background = DefaultBackground;
        }
    }
}