

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using System.Diagnostics;

namespace ShopErp.App.Views.Shops
{
    /// <summary>
    /// ShopUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ShopUserControl : UserControl
    {
        public ShopUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = OperatorService.LoginOperator.Rights.Contains("店铺管理");
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.dgvOperators.ItemsSource = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            new ShopEditWindow().ShowDialog();
            this.btnRefresh_Click(null, null);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dgvOperators.SelectedCells.Count < 1)
                {
                    throw new Exception("未选择数据");
                }
                var shop = this.dgvOperators.SelectedCells[0].Item as Shop;
                if (shop == null)
                {
                    throw new InvalidProgramException("选择的数据不为:" + typeof(Shop).FullName);
                }
                new ShopEditWindow { Shop = shop }.ShowDialog();
                this.btnRefresh_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dgvOperators.SelectedCells.Count < 1)
                {
                    throw new Exception("未选择数据");
                }
                var shop = this.dgvOperators.SelectedCells[0].Item as Shop;
                if (shop == null)
                {
                    throw new InvalidProgramException("选择的数据不为:" + typeof(Shop).FullName);
                }
                string msg = "是否删除店铺:" + shop.PopSellerId;
                if (MessageBox.Show(msg, "警告", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
                if (MessageBox.Show(msg, "警告", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
                ServiceContainer.GetService<ShopService>().Delete(shop.Id);
                this.btnRefresh_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenUrl_Click2(object sender, EventArgs e)
        {
            try
            {
                TextBlock tb = sender as TextBlock;
                if (tb == null)
                {
                    throw new Exception("Edit_Click错误，事件源不是TextBlock");
                }
                var shop = tb.DataContext as Shop;
                var url = ServiceContainer.GetService<ShopService>().GetShopOauthUrl(shop.Id).data;
                Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}