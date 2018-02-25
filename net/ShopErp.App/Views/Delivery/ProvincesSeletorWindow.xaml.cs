using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// ProvincesSeletorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProvincesSeletorWindow : Window
    {
        public string Province { get; set; }

        public ProvincesSeletorWindow()
        {
            InitializeComponent();
        }

        private List<CheckBox> FindChildren()
        {
            List<CheckBox> children = new List<CheckBox>();

            foreach (var v in this.dpHost.Children.OfType<StackPanel>())
            {
                foreach (var vv in (v as StackPanel).Children.OfType<CheckBox>())
                {
                    if (vv.GetType() == typeof(CheckBox))
                    {
                        children.Add(vv as CheckBox);
                    }
                }
            }
            return children;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckBox[] cbs = this.FindChildren().ToArray();
            if (this.Province != null)
            {
                foreach (var ch in cbs)
                {
                    if (this.Province.Contains(ch.Content.ToString()))
                    {
                        ch.IsChecked = true;
                    }
                }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            CheckBox[] cbs = this.FindChildren().Where(obj => obj.IsChecked != null && obj.IsChecked.Value).ToArray();
            string[] s = cbs.Select(obj => obj.Content.ToString()).ToArray();
            this.Province = string.Join(",", s);
            this.DialogResult = true;
        }
    }
}