

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
using ShopErp.Domain;

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// Interaction logic for GoodsEditMapWindow.xaml
    /// </summary>
    public partial class GoodsEditMapWindow : Window
    {
        public long GoodsId { get; set; }

        public GoodsEditMapWindow()
        {
            InitializeComponent();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dgvGoodsMap.SelectedCells.Count < 1)
                {
                    throw new Exception("先选择货号");
                }
                var item = this.dgvGoodsMap.SelectedCells[0].Item as GoodsMap;
                if (item == null)
                {
                    throw new Exception("先选择货号");
                }
                if (MessageBox.Show("是否删除:" + item.Number + "?", "警告", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                ServiceContainer.GetService<GoodsMapService>().Delete(item.Id);
                this.Window_Loaded(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new GoodsAddMapWindow {GoodsId = this.GoodsId}.ShowDialog();
                this.Window_Loaded(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var gps = ServiceContainer.GetService<GoodsMapService>().GetByAll("", "", this.GoodsId, 0, 0);
                this.dgvGoodsMap.ItemsSource = gps.Datas;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }
    }
}