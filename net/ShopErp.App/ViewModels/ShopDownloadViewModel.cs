using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ShopErp.Domain;

namespace ShopErp.App.ViewModels
{
    public class ShopDownloadViewModel : DependencyObject
    {
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(ShopDownloadViewModel));

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(ShopDownloadViewModel));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(ShopDownloadViewModel));

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(ShopDownloadViewModel));

        public bool IsChecked
        {
            get { return (bool) this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        public double Progress
        {
            get { return (double) GetValue(ProgressProperty); }
            set { this.SetValue(ProgressProperty, value); }
        }

        public string Message
        {
            get { return (string) GetValue(MessageProperty); }
            set { this.SetValue(MessageProperty, value); }
        }

        public Brush Background
        {
            get { return (Brush) GetValue(BackgroundProperty); }
            set { this.SetValue(BackgroundProperty, value); }
        }

        public Shop Source { get; set; }

        public ShopDownloadViewModel(Shop s)
        {
            this.Source = s;
            Progress = 0;
        }
    }
}