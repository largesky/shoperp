 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// GoodsDetailWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GoodsDetailWindow : Window
    {
        public ShopErp.Domain.Goods Goods { get; set; }

        public GoodsDetailWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Goods != null)
            {
                this.dgvUploadShops.ItemsSource = Goods.Shops;
            }
        }
    }
}