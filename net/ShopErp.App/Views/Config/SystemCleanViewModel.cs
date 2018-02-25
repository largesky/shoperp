using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ShopErp.App.Service.Restful;

namespace ShopErp.App.Views.Config
{
    class SystemCleanViewModel : DependencyObject
    {
        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(string),
            typeof(SystemCleanViewModel), new PropertyMetadata("1970-01-01 00:00:01"));

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(string), typeof(SystemCleanViewModel));

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(long), typeof(SystemCleanViewModel));

        public static readonly DependencyProperty ToCleanCountProperty =
            DependencyProperty.Register("ToCleanCount", typeof(long), typeof(SystemCleanViewModel));

        public string Title { get; set; }

        public string TableName { get; set; }

        public string Time
        {
            get { return (string)this.GetValue(TimeProperty); }
            set { this.SetValue(TimeProperty, value); }
        }

        public string State
        {
            get { return (string)this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }

        public long Count
        {
            get { return (long)this.GetValue(CountProperty); }
            set { this.SetValue(CountProperty, value); }
        }

        public long ToCleanCount
        {
            get { return (long)this.GetValue(ToCleanCountProperty); }
            set { this.SetValue(ToCleanCountProperty, value); }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == TimeProperty)
            {
                if (string.IsNullOrWhiteSpace(Time))
                {
                    return;
                }
                try
                {
                    ToCleanCount = ServiceContainer.GetService<SystemCleanService>().GetTableCount(TableName, DateTime.Parse(Time)).data;
                }
                catch (Exception ex)
                {
                    State = ex.Message;
                }
            }
        }
    }
}