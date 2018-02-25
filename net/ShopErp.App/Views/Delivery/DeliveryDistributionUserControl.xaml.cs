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
using Microsoft.Win32;
using System.Diagnostics;
using ShopErp.App.ViewModels;
using ShopErp.App.Domain;
using ShopErp.App.Service.Net;
using ShopErp.App.Service.Restful;
using ShopErp.App.Views.Orders;
using ShopErp.Domain;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// AddressCheckUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class DeliveryDistributionUserControl : UserControl
    {
        private List<OrderViewModel> orders = new List<OrderViewModel>();
        private bool isLoaded = false;

        public DeliveryDistributionUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.isLoaded)
                {
                    return;
                }

                try
                {
                    MessageCenter.Instance.Start();
                    MessageCenter.Instance.MessageArrived += new EventHandler<MessageArrviedEventArgs>(Instance_MessageArrived);
                }
                catch (Exception ee)
                {
                    var ie = ee;
                    while (ie.InnerException != null)
                    {
                        ie = ie.InnerException;
                    }
                    MessageBox.Show("初始化网络同步端口失败，你在快递分配的操作将无法实时更新到其它电脑" + Environment.NewLine + ie.Message, "错误");
                }

                //生成快递右键菜单
                var ptcs = DeliveryCompanyService.GetDeliveryCompaniyNames();
                foreach (var ptc in ptcs)
                {
                    MenuItem miDeliveryCompanyChose = new MenuItem { Header = ptc, Tag = ptc };
                    miDeliveryCompanyChose.Click += miDeliveryCompanyChose_Click;
                    this.dgvOrders.ContextMenu.Items.Insert(this.dgvOrders.ContextMenu.Items.Count - 2, miDeliveryCompanyChose);
                }
                isLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void miDeliveryCompanyChose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                string tag = fe.Tag == null ? "" : fe.Tag.ToString();
                if (string.IsNullOrWhiteSpace(tag))
                {
                    throw new Exception("没有Tag数据");
                }

                if (this.dgvOrders.SelectedCells.Count < 1)
                {
                    throw new Exception("没有选择数据");
                }

                var orders = this.dgvOrders.SelectedCells.Select(obj => obj.Item as OrderViewModel).Distinct();
                foreach (var order in orders)
                {
                    try
                    {
                        var o = ServiceContainer.GetService<OrderService>().GetById(order.Source.Id);
                        if (string.IsNullOrWhiteSpace(o.DeliveryNumber) == false)
                        {
                            throw new Exception("订单已经打印不允许修改:" + o.State);
                        }

                        order.Source.DeliveryCompany = tag;
                        order.DeliveryCompany = tag;
                        ServiceContainer.GetService<OrderService>().Update(order.Source);
                        this.UpdateSum();
                        OrderDeliveryInfoChangedMessage m = new OrderDeliveryInfoChangedMessage
                        {
                            Time = DateTime.Now,
                            DeliveryNumber = "",
                            DeliveryCompany = tag,
                            OrderId = order.Source.Id,
                            SenderId = "",
                            SenderName = "",
                            Targets = new string[0],
                        };
                        MessageCenter.Instance.SendMessage(m);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "订单编号:" + order.Source.Id + "设置快递出错");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageCenter.Instance.MessageArrived -= new EventHandler<MessageArrviedEventArgs>(Instance_MessageArrived);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }

        void Instance_MessageArrived(object sender, MessageArrviedEventArgs e)
        {
            if (e.Message == null)
            {
                return;
            }

            if (e.Message is OrderPopSellerCommentChangedMessage)
            {
                OrderPopSellerCommentChangedMessage m = e.Message as OrderPopSellerCommentChangedMessage;
                this.Dispatcher.BeginInvoke(new Action(() => this.UpdateOrderFromMessage(m)));
            }
            else if (e.Message is OrderDeliveryInfoChangedMessage)
            {
                OrderDeliveryInfoChangedMessage m = e.Message as OrderDeliveryInfoChangedMessage;
                this.Dispatcher.BeginInvoke(new Action(() => this.UpdateOrderDeliveryInfoFromMessage(m)));
            }
        }

        private OrderViewModel GetCurrentSelectedModel()
        {
            if (this.dgvOrders.SelectedCells.Count < 1)
            {
                MessageBox.Show("未选择订单");
                return null;
            }
            return this.dgvOrders.SelectedCells[0].Item as OrderViewModel;
        }

        private void UpdateOrderFromMessage(OrderPopSellerCommentChangedMessage message)
        {
            try
            {
                OrderViewModel order = this.orders.FirstOrDefault(obj => obj.Source.Id == message.OrderId);
                if (order == null)
                {
                    return;
                }
                order.Source.PopFlag = message.Flag;
                order.Source.PopSellerComment = message.SellerComment;
                order.PopSellerComment = message.SellerComment;
                order.OrderFlag = message.Flag;
            }
            catch
            {
            }
        }

        private void UpdateOrderDeliveryInfoFromMessage(OrderDeliveryInfoChangedMessage message)
        {
            try
            {
                OrderViewModel order = this.orders.FirstOrDefault(obj => obj.Source.Id == message.OrderId);
                if (order == null)
                {
                    return;
                }
                order.Source.DeliveryCompany = message.DeliveryCompany;
                order.Source.DeliveryNumber = message.DeliveryNumber;
                order.DeliveryCompany = message.DeliveryCompany;
                order.DeliveryNumber = message.DeliveryNumber;
                this.UpdateSum();
            }
            catch
            { }
        }

        private void UpdateOrder(OrderViewModel order, ColorFlag flag, string comment, bool appendAlready = true)
        {
            try
            {
                if (order == null)
                {
                    return;
                }
                comment += string.Format("【{0} {1}】", OperatorService.LoginOperator.Name[0],
                    DateTime.Now.ToString("MM-dd HH:mm"));
                if (appendAlready && string.IsNullOrWhiteSpace(order.PopSellerComment) == false)
                {
                    comment = order.PopSellerComment + " " + comment;
                }
                ServiceContainer.GetService<OrderService>().ModifyPopSellerComment(order.Source.Id, flag, comment);
                order.OrderFlag = flag;
                order.PopSellerComment = comment;
                order.Source.PopSellerComment = comment;
                order.Source.PopFlag = flag;
                ServiceContainer.GetService<OrderService>().Update(order.Source);
                MessageCenter.Instance.SendMessage(new OrderPopSellerCommentChangedMessage
                {
                    Flag = flag,
                    SellerComment = comment,
                    OrderId = order.Source.Id,
                    SenderName = "",
                    Targets = new string[0],
                    Time = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "更新失败");
            }
        }

        private void UpdateSelectedOrder(ColorFlag flag, string comment, bool appendAlreay = true)
        {
            try
            {
                this.UpdateOrder(this.GetCurrentSelectedModel(), flag, comment, appendAlreay);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "更新失败");
            }
        }

        private void startManualEdit_Click(object sender, RoutedEventArgs e)
        {
            var order = this.GetCurrentSelectedModel();
            if (order == null)
            {
                return;
            }
            OrderCommentAndFlagEditWindow or = new OrderCommentAndFlagEditWindow { Flag = order.Source.PopFlag, Comment = order.Source.PopSellerComment };
            if (or.ShowDialog().Value)
            {
                this.UpdateOrder(order, or.Flag, or.Comment, false);
            }
        }

        private void UpdateSum()
        {
            if (this.orders == null || this.orders.Count < 0)
            {
                this.tbTotal.Text = "没有订单";
                return;
            }
            string message = "订单总数:" + this.orders.Count + ",";
            var group = this.orders.GroupBy(obj => obj.DeliveryCompany).ToArray();
            string ss = string.Join(",",
                group.Select(obj => (string.IsNullOrWhiteSpace(obj.Key) ? "未分配" : obj.Key) + ":" + obj.Count()));
            this.tbTotal.Text = message + ss;
        }

        /// <summary>
        /// Handles the Click event of the btnRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var os = OrderDownloadWindow.DownloadOrder(PopPayType.ONLINE);
                var orders1 = os.Where(obj => obj.State == OrderState.PAYED).ToArray();
                if (this.chkIncludeDis.IsChecked == false)
                {
                    orders1 = os.Where(obj => string.IsNullOrWhiteSpace(obj.DeliveryCompany)).ToArray();
                }
                var orders = orders1.Select(obj => new OrderViewModel(obj)).ToArray();
                if (orders.Length < 1)
                {
                    this.dgvOrders.ItemsSource = null;
                    this.orders.Clear();
                    MessageBox.Show("没有找到需要检查地址的订单");
                    return;
                }

                //分析
                foreach (var order in orders)
                {
                    //检查地址中是否含村
                    if (order.Source.ReceiverAddress.Contains('村'))
                    {
                        order.Background = Brushes.LightBlue;
                    }

                    //读取本地历史订单
                    var localHistoryOrders = ServiceContainer.GetService<OrderService>().GetOrdersByInfoIdNotEqual(
                        order.Source.PopBuyerId, order.Source.ReceiverPhone, order.Source.ReceiverMobile
                        , order.Source.ReceiverAddress, order.Source.Id);
                    order.HistoryOrders = localHistoryOrders.Datas
                        .Where(obj => orders.Any(ov => ov.Source.Id == obj.Id) == false)
                        .Select(obj => new OrderViewModel(obj)).ToList();
                }
                this.orders.Clear();
                this.orders.AddRange(orders);
                this.dgvOrders.ItemsSource = orders;
                this.UpdateSum();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void dgvOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (this.dgvOrders.SelectedCells.Count < 1)
                {
                    this.dgvHistoryItem.ItemsSource = null;
                    return;
                }
                this.dgvHistoryItem.ItemsSource = (this.dgvOrders.SelectedCells[0].Item as OrderViewModel)
                    .HistoryOrders;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                this.dgvHistoryItem.ItemsSource = null;
            }
        }

        private void historyStartManualEdit_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgvHistoryItem.SelectedCells.Count < 1)
            {
                return;
            }
            var order = this.dgvHistoryItem.SelectedCells[0].Item as OrderViewModel;
            if (order == null)
            {
                return;
            }
            OrderCommentAndFlagEditWindow or = new OrderCommentAndFlagEditWindow { Flag = order.OrderFlag, Comment = order.PopSellerComment };
            if (or.ShowDialog().Value)
            {
                this.UpdateOrder(order, or.Flag, or.Comment);
            }
        }
    }
}