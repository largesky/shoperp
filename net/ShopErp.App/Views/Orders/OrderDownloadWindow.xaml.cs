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
using System.Threading;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// Interaction logic for OrderDownloadWindow.xaml
    /// </summary>
    public partial class OrderDownloadWindow : Window
    {
        public PopPayType PayType { get; set; }
        public string Shipper { get; set; }
        public List<Order> Orders { get { return this.allOrders; } }
        public bool UserStop { get; set; }

        private Task task = null;
        private bool hasError = false;
        private List<ShopDownloadViewModel> shopVms = new List<ShopDownloadViewModel>();
        private Dictionary<Shop, List<Order>> shopOrders = new Dictionary<Shop, List<Order>>();
        private List<Order> allOrders = new List<Order>();
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

                foreach (var shop in shops)
                {
                    shopVms.Add(new ShopDownloadViewModel(shop));
                    shopOrders.Add(shop, new List<Order>());
                }
                this.lstShops.ItemsSource = this.shopVms;
                this.dgvFailOrders.ItemsSource = failOrders;
                this.task = Task.Factory.StartNew(DownloadTask);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.DialogResult = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.task == null)
            {
                return;
            }

            if (MessageBox.Show("正在下载中，是否停止?", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            this.UserStop = true;
            this.task.Wait();
        }

        private void DownloadTask()
        {
            try
            {
                this.hasError = false;
                Task.WaitAll(shopOrders.Keys.Select(obj => Task.Factory.StartNew(() => DownloadOneShopTask(obj))).ToArray());
                //有时候没有订单下载，执行太快，给人没人执行的感觉
                Thread.Sleep(2000);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                this.hasError = true;
            }
            finally
            {
                this.task = null;
                foreach (var v in shopOrders)
                {
                    this.allOrders.AddRange(v.Value);
                }
                if (this.hasError == false)
                {
                    this.Dispatcher.BeginInvoke(new Action(() => this.Close()));
                }
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
            int pageIndex = 0, pageSize = 20;
            try
            {
                var os = ServiceContainer.GetService<OrderService>();

                while (this.UserStop == false)
                {
                    this.UpdateShopState(shop, false, 0, 0, string.Format("每页{0}条订单，正在下载第{1}页", pageSize, pageIndex + 1), null);
                    var ret = os.GetPopWaitSendOrders(shop, PayType, pageIndex, pageSize);
                    if (ret.Datas == null || ret.Datas.Count < 1)
                    {
                        break;
                    }
                    //添加下载结果
                    foreach (var v in ret.Datas)
                    {
                        if (v.Error != null)
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                failOrders.Add(v.Error);
                            }));
                        }
                        else if (v.Order != null)
                        {
                            if (this.shopOrders[shop].FirstOrDefault(obj => obj.PopOrderId == v.Order.PopOrderId) == null)
                            {
                                this.shopOrders[shop].Add(v.Order);
                            }
                        }
                        else
                        {
                            throw new Exception("服务端返回有Error和Order都为空的对象");
                        }
                    }

                    this.hasError = this.hasError ? true : ret.Datas.Any(obj => obj.Error != null);
                    this.UpdateShopState(shop, ret.IsTotalValid, ret.Total, this.shopOrders[shop].Count, string.Format("每页{0}条订单，已下载第{1}页", pageSize, pageIndex + 1), null);
                    pageIndex++;
                }
                if (this.shopOrders[shop].Count == 0)
                {
                    this.UpdateShopState(shop, true, 1, 0, "没有订单", null);
                }
                else
                {
                    this.UpdateShopState(shop, true, 1, 1, "已成功下载订单：" + this.shopOrders[shop].Count, null);
                }
            }
            catch (Exception e)
            {
                this.hasError = true;
                this.UpdateShopState(shop, true, 1, 1, e.Message, Brushes.Red);
            }
        }

        /// <summary>
        /// 获取已付款和已打印的订单，同时会调用接口下载订单
        /// </summary>
        /// <param name="payType">支付类型</param>
        /// <returns></returns>
        public static List<Order> DownloadOrder(PopPayType payType, string shipper)
        {
            var allShops = ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.Enabled).ToList();
            var allAppEnabledShops = allShops.Where(obj => obj.AppEnabled).ToArray();
            var allAppUnEnabledShops = allShops.Where(obj => obj.AppEnabled == false).ToArray();

            if (allShops.Count < 1)
            {
                return new List<Order>();
            }

            string mode = LocalConfigService.GetValue(SystemNames.CONFIG_ORDER_DOWNLOAD_MODE, "").Trim();
            if (mode.Equals("本地读取"))
            {
                return ServiceContainer.GetService<OrderService>().GetPayedAndPrintedOrders(null, OrderCreateType.NONE, payType, shipper, 0, 0).Datas.OrderBy(obj => obj.ShopId).ToList();
            }

            OrderDownloadWindow win = new OrderDownloadWindow() { PayType = payType };
            win.ShowDialog();
            if (win.UserStop)
            {
                return new List<Order>();
            }
            var onlineOrders = win.Orders.Where(obj => obj.PopPayType == payType).ToList();

            if (allAppEnabledShops.Length > 0)
            {
                //对于采用自动下载订单的店铺，需要再读取其手动创建的订单
                var orders = ServiceContainer.GetService<OrderService>().GetPayedAndPrintedOrders(allAppEnabledShops.Select(obj => obj.Id).ToArray(), OrderCreateType.MANUAL, payType, shipper, 0, 0).Datas;
                if (orders.Count > 0)
                {
                    onlineOrders.AddRange(orders);
                }
            }

            if (allAppUnEnabledShops.Length > 0)
            {
                //对于没有自动下载的订单需要读取所有的订单
                var orders = ServiceContainer.GetService<OrderService>().GetPayedAndPrintedOrders(allAppUnEnabledShops.Select(obj => obj.Id).ToArray(), OrderCreateType.NONE, payType, shipper, 0, 0).Datas;
                if (orders.Count > 0)
                {
                    onlineOrders.AddRange(orders);
                }
            }
            if (string.IsNullOrWhiteSpace(shipper))
            {
                return onlineOrders;
            }

            var retOrders = new List<Order>();
            foreach (var o in onlineOrders)
            {
                if (o.OrderGoodss == null || o.OrderGoodss.Count < 0)
                {
                    continue;
                }

                if (o.OrderGoodss.Any(obj => obj.Shipper.Equals(shipper, StringComparison.OrdinalIgnoreCase)))
                {
                    retOrders.Add(o);
                }
            }

            return retOrders;
        }
    }
}