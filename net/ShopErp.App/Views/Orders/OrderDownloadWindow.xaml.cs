using ShopErp.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.App.Utils;
using ShopErp.Domain;
using ShopErp.Domain.Pop;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// Interaction logic for OrderDownloadWindow.xaml
    /// </summary>
    public partial class OrderDownloadWindow : Window
    {
        public PopPayType PayType { get; set; }

        public List<Order> Orders
        {
            get
            {
                var ors = new List<Order>();
                foreach (var v in this.orders.Values)
                {
                    ors.AddRange(v);
                }
                return ors;
            }
        }
        private Task task = null;
        private bool hasError = false;
        private List<ShopDownloadViewModel> shopVms = new List<ShopDownloadViewModel>();
        private Dictionary<Shop, List<Order>> orders = new Dictionary<Shop, List<Order>>();
        private System.Collections.ObjectModel.ObservableCollection<OrderDownloadError> failOrders = new System.Collections.ObjectModel.ObservableCollection<OrderDownloadError>();

        public OrderDownloadWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.Enabled && obj.AppEnabled).ToArray();
                shopVms.AddRange(shops.Select(obj => new ShopDownloadViewModel(obj)));
                foreach (var shop in shops)
                {
                    orders.Add(shop, new List<Order>());
                }
                this.lstShops.ItemsSource = this.shopVms;
                this.dgvFailOrders.ItemsSource = failOrders;
                this.Dispatcher.BeginInvoke(new Action(() => this.btnStart_Click(this.btnStart, new RoutedEventArgs())));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.DialogResult = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.task != null && this.task.Status != TaskStatus.RanToCompletion)
            {
                this.task.Wait();
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            this.failOrders.Clear();
            foreach (var v in orders)
            {
                v.Value.Clear();
            }
            foreach (var v in shopVms)
            {
                v.Background = null;
                v.Message = "";
                v.Progress = 0;
            }
            this.task = Task.Factory.StartNew(DownloadTask);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (this.DialogResult == null)
                this.DialogResult = true;
        }

        private void DownloadTask()
        {
            try
            {
                this.hasError = false;
                this.Dispatcher.BeginInvoke(new Action(() => this.Title = "订单下载："));
                this.Dispatcher.BeginInvoke(new Action(() => this.btnStart.IsEnabled = false));
                Task.WaitAll(orders.Keys.Select(obj => Task.Factory.StartNew(() => DownloadOneShopTask(obj))).ToArray());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                this.hasError = true;
            }
            finally
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (this.DialogResult == null)
                        {
                            this.Title = "订单下载：";
                            this.btnStart.IsEnabled = true;
                            if (this.hasError == false)
                            {
                                this.DialogResult = true;
                            }
                        }
                    }
                    catch
                    {
                    }
                }));
            }
        }

        private void UpdateShopState(Shop shop, bool isTotalValid, int total, int current, string message, Brush backgroud)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                var sv = shopVms.FirstOrDefault(obj => obj.Source.Id == shop.Id);
                sv.Background = backgroud;
                sv.Message = message;
                total = total == 0 ? 1 : total;
                if (isTotalValid && current > 0)
                {
                    sv.Progress = current * 1F / total * 100;
                }
                else
                {
                    sv.Progress = 0;
                }
            }));
        }

        private void DownloadOneShopTask(Shop shop)
        {
            int pageIndex = 0, pageSize = 20, downloaded = 0;
            try
            {
                var os = ServiceContainer.GetService<OrderService>();

                while (true)
                {
                    this.UpdateShopState(shop, false, 0, 0, string.Format("每页{0}条订单，正在下载第{1}页", pageSize, pageIndex + 1), null);
                    var ret = os.GetPopWaitSendOrders(shop, PayType, pageIndex, pageSize);
                    if (ret.Datas == null || ret.Datas.Count < 1)
                    {
                        break;
                    }
                    this.orders[shop].AddRange(ret.Datas.Where(obj => obj.Order != null).Select(obj => obj.Order).ToArray());
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (var v in ret.Datas.Where(obj => obj.Error != null).Select(obj => obj.Error).ToArray())
                        {
                            failOrders.Add(v);
                        }
                    }));
                    this.hasError = this.hasError ? true : ret.Datas.Any(obj => obj.Error != null);
                    downloaded += ret.Datas.Count;
                    this.UpdateShopState(shop, ret.IsTotalValid, ret.Total, downloaded, string.Format("每页{0}条订单，已下载第{1}页", pageSize, pageIndex + 1), null);
                    pageIndex++;
                }
            }
            catch (Exception e)
            {
                this.hasError = true;
                this.UpdateShopState(shop, true, 1, 1, e.Message, Brushes.Red);
            }
            finally
            {
                if (downloaded == 0)
                {
                    this.UpdateShopState(shop, true, 1, 0, "没有订单", null);
                }
                else
                {
                    this.UpdateShopState(shop, true, 1, 1, "已成功下载订单：" + downloaded, null);
                }
                this.task = null;
            }
        }


        /// <summary>
        /// 下载待发货订单
        /// </summary>
        /// <param name="payType">支付类型</param>
        /// <returns></returns>
        public static List<Order> DownloadOrder(PopPayType payType)
        {
            string mode = LocalConfigService.GetValue(SystemNames.CONFIG_ORDER_DOWNLOAD_MODE, "").Trim();
            if (mode.Equals("本地读取"))
            {
                return ServiceContainer.GetService<OrderService>().GetPayedAndPrintedOrders(null, OrderCreateType.NONE, payType, 0, 0).Datas.OrderBy(obj => obj.ShopId).ToList();
            }

            OrderDownloadWindow win = new OrderDownloadWindow() { PayType = payType };
            var ret = win.ShowDialog();
            if (ret == null || ret.Value == false)
            {
                return new List<Order>();
            }

            var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.Enabled).ToList();


            var onlineOrders = win.Orders.Where(obj => obj.PopPayType == payType).ToList();

            //对于采用自动下载订单的店铺，需要再读取其手动创建的订单
            var orders = ServiceContainer.GetService<OrderService>().GetPayedAndPrintedOrders(shops.Where(obj => win.shopVms.FirstOrDefault(o => o.Source.Id == obj.Id) != null).Select(obj => obj.Id).ToArray(), OrderCreateType.MANUAL, payType, 0, 0).Datas;
            if (orders.Count > 0)
                onlineOrders.AddRange(orders);
            //对于没有自动下载的订单需要读取所有的订单
            orders = ServiceContainer.GetService<OrderService>().GetPayedAndPrintedOrders(shops.Where(obj => win.shopVms.FirstOrDefault(o => o.Source.Id == obj.Id) == null).Select(obj => obj.Id).ToArray(), OrderCreateType.NONE, payType, 0, 0).Datas;
            if (orders.Count > 0)
                onlineOrders.AddRange(orders);

            return onlineOrders;
        }
    }
}