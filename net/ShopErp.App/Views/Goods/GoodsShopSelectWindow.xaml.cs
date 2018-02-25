
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
using ShopErp.App.Service.Restful;

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// Interaction logic for GoodsCompleteWindow.xaml
    /// </summary>
    public partial class GoodsShopSelectWindow : Window
    {
        public ShopErp.Domain.Goods Goods { get; set; }

        public long[] SelectedShops { get; set; }

        public long[] InitSelectedShopIds { get; set; }

        public GoodsShopSelectWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
                var shopvms = new List<ShopCheckViewModel>();
                if (this.Goods.Shops != null)
                {
                    foreach (var s in this.Goods.Shops)
                    {
                        var v = new ShopCheckViewModel(shops.FirstOrDefault(obj => obj.Id == s.ShopId));
                        v.IsChecked = InitSelectedShopIds == null ? false : InitSelectedShopIds.Contains(s.ShopId);
                        shopvms.Add(v);
                    }
                }
                this.cbbShops.ItemsSource = shopvms;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.SelectedShops = (this.cbbShops.ItemsSource as List<ShopCheckViewModel>).Where(obj => obj.IsChecked)
                    .Select(obj => obj.Source.Id).ToArray();
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}