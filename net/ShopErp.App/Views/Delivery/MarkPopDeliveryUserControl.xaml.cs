using ShopErp.App.ViewModels;
using ShopErp.App.Views.Orders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// Interaction logic for DeliveryCheckUserControl.xaml
    /// </summary>
    public partial class MarkPopDeliveryUserControl : UserControl
    {
        private System.Collections.ObjectModel.ObservableCollection<OrderViewModel> orders = new ObservableCollection<OrderViewModel>();

        public MarkPopDeliveryUserControl()
        {
            InitializeComponent();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.orders.Clear();
                var showShops = ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.Enabled && obj.AppEnabled).ToList();
                var showShopIds = showShops.Select(obj => obj.Id).ToArray();
                var downloadOrders = OrderDownloadWindow.DownloadOrder(PopPayType.ONLINE, "");
                if (downloadOrders == null || downloadOrders.Count < 1)
                {
                    return;
                }
                var orders = downloadOrders.Where(obj => string.IsNullOrWhiteSpace(obj.PopOrderId) == false && showShopIds.Contains(obj.ShopId)).Select(obj => new OrderViewModel(obj)).OrderBy(obj => obj.Source.PopPayTime).ToArray();
                if (orders.Length < 1)
                {
                    return;
                }
                //分析
                foreach (var order in orders)
                {
                    var time = DateTime.Now.Subtract(order.Source.PopPayTime).TotalHours;
                    var sTime = showShops.FirstOrDefault(obj => obj.Id == order.Source.ShopId).ShippingHours;
                    if (time >= sTime)
                    {
                        order.Background = Brushes.Red;
                        order.IsChecked = true;
                    }
                    else if (time - sTime >= -1)
                    {
                        order.Background = Brushes.Yellow;
                        order.IsChecked = true;
                    }
                    this.orders.Add(order);
                }
                this.dgvOrders.ItemsSource = this.orders;
                this.tbTotal.Text = "当前共 : " + orders.Length + " 条记录";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void btnMarkDelivery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var so = this.orders.Where(obj => obj.IsChecked).ToArray();
                if (so.Length < 1)
                {
                    throw new Exception("没有选择订单");
                }
                var os = ServiceContainer.GetService<OrderService>();
                foreach (var o in so)
                {
                    WPFHelper.DoEvents();
                    try
                    {
                        os.MarkPopDelivery(o.Source.Id, "");
                        o.State = "标记成功";
                        o.Background = null;
                    }
                    catch (Exception ex)
                    {
                        o.State = ex.Message;
                        o.Background = Brushes.Red;
                    }
                }
                MessageBox.Show("所有订单标记完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private OrderViewModel GetMIOrder(object sender)
        {
            MenuItem mi = sender as MenuItem;
            var dg = ((ContextMenu)mi.Parent).PlacementTarget as DataGrid;
            var cells = dg.SelectedCells;
            if (cells.Count < 1)
            {
                throw new Exception("未选择数据");
            }

            var item = cells[0].Item as OrderViewModel;
            if (item == null)
            {
                throw new Exception("数据对象不正确");
            }
            return item;
        }

        private void miSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.GetMIOrder(sender);
                MenuItem mi = sender as MenuItem;
                var orders = this.orders;
                int index = orders.IndexOf(item);
                bool isPre = mi.Header.ToString().Contains("向前选择");
                for (int i = 0; i < orders.Count; i++)
                {
                    orders[i].IsChecked = isPre ? (i <= index ? true : false) : (i >= index ? true : false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (orders == null || orders.Count < 1)
                {
                    return;
                }
                bool isChecked = ((CheckBox)sender).IsChecked.Value;
                foreach (var item in orders)
                {
                    item.IsChecked = isChecked;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}