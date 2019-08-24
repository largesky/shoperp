using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ShopErp.App.Views.Config
{
    public class ImgCleanViewModel : DependencyObject
    {
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(string), typeof(ImgCleanViewModel));

        public static readonly DependencyProperty CheckProperty = DependencyProperty.Register("Check", typeof(bool), typeof(ImgCleanViewModel));

        public ShopErp.Domain.Goods Goods { get; set; }

        public string Dir { get; set; }

        public string State
        {
            get { return (string)this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }

        public bool Check
        {
            get { return (bool)this.GetValue(CheckProperty); }
            set { this.SetValue(CheckProperty, value); }
        }
    }
}