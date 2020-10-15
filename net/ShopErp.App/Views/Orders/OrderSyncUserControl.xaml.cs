using ShopErp.App.Domain.TaobaoHtml.Order;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.App.Utils;
using ShopErp.Domain;
using ShopErp.Domain.Pop;
using System.Text.RegularExpressions;
using ShopErp.App.CefSharpUtils;
using ShopErp.App.Service.Net;
using ShopErp.App.Views.Extenstions;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// Interaction logic for OrderSyncUserControl.xaml
    /// </summary>
    public partial class OrderSyncUserControl : UserControl
    {
        private bool isRunning = false;
        private bool isStop = false;
        private bool myLoaded = false;

        public OrderSyncUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.myLoaded)
                {
                    return;
                }
                this.dpStart.Value = DateTime.Now.AddDays(-10);
                this.dpEnd.Value = DateTime.Now;
                this.cbbTypes.Bind<OrderType>();
                var ret = ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.Enabled).ToList();
                ret.Insert(0, new Shop { Mark = "所有", Id = 0, Enabled = true });
                this.cbbShops.ItemsSource = ret;
                this.cbbShops.SelectedIndex = 0;
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AppendText(string text)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (this.tbMessage.LineCount > 10000)
                {
                    this.tbMessage.Text = "";
                }
                this.tbMessage.AppendText(text);
                this.tbMessage.ScrollToEnd();
            }));
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.isRunning)
                {
                    this.isStop = true;
                    return;
                }
                this.isStop = false;
                this.isRunning = true;
                string popOrderId = this.tbPopOrderId.Text.Trim();
                var dt = this.dpStart.Value.Value;
                var end = this.dpEnd.Value.Value;
                var ot = this.cbbTypes.GetSelectedEnum<OrderType>();
                Shop[] shops = null;
                var selectedShop = this.cbbShops.SelectedItem as Shop;
                if (selectedShop == null || selectedShop.Id == 0)
                {
                    shops = this.cbbShops.ItemsSource.OfType<Shop>().Where(obj => obj.Id > 0).ToArray();
                }
                else
                {
                    shops = new Shop[] { selectedShop };
                }
                Task.Factory.StartNew(new Action(() => Start(shops, popOrderId, ot, dt, end)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Start(Shop[] shops, string popOrderId, OrderType orderType, DateTime startTime, DateTime endTime)
        {
            try
            {
                if (this.isStop)
                {
                    this.isRunning = false;
                    return;
                }
                this.isRunning = true;
                this.Dispatcher.BeginInvoke(new Action(() => this.btnUpdate.Content = "停止"));
                var ors = ServiceContainer.GetService<OrderUpdateService>().GetByAll(shops.Select(obj => obj.Id).ToArray(), popOrderId, orderType, startTime, endTime, 0, 0);
                var orders = ors.Datas.Where(obj => string.IsNullOrWhiteSpace(obj.PopOrderId) == false).ToArray();
                if (orders.Length < 1)
                {
                    throw new Exception("订单不存在");
                }
                int i = 0;
                foreach (var o in orders)
                {
                    if (this.isStop)
                    {
                        return;
                    }
                    this.AppendText(DateTimeUtil.FormatDateTime(DateTime.Now) + ":" + "正在下载订单状态: " + (++i) + " / " + orders.Count() + "  " + o.PopOrderId + "  ");
                    Shop shop = shops.FirstOrDefault(obj => obj.Id == o.ShopId);
                    string ret = "";
                    var state = OrderState.NONE;
                    if (shop.AppEnabled)
                    {
                        state = ServiceContainer.GetService<OrderService>().GetPopOrderState(shop, o.PopOrderId).First.State;
                    }
                    else
                    {
                        state = MainWindow.ProgramMainWindow.QueryUserControlInstance<AttachUI.Taobao.TaobaoUserControl>().GetOrderState(o.PopOrderId).State;
                    }
                    ret = ServiceContainer.GetService<OrderService>().UpdateOrderState(o.PopOrderId, state, o, shop).data;
                    this.AppendText(ret + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                this.AppendText(ex.Message);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.isRunning = false;
                this.isStop = false;
                this.Dispatcher.BeginInvoke(new Action(() => this.btnUpdate.Content = "开始同步"));
            }
        }
    }
}