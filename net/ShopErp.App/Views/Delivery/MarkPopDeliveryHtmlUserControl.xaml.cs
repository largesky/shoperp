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
using ShopErp.App.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ShopErp.App.CefSharpUtils;
using ShopErp.App.Service.Net;
using System.ComponentModel;
using System.Web;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// Interaction logic for OrderSyncUserControl.xaml
    /// </summary>
    public partial class MarkPopDeliveryHtmlUserControl : UserControl
    {
        private bool isRunning = false;
        private ObservableCollection<OrderViewModel> orders = new ObservableCollection<OrderViewModel>();
        private List<OrderDownload> orderDownloads = null;

        public MarkPopDeliveryHtmlUserControl()
        {
            InitializeComponent();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            AttachUI.TaobaoUserControl taobaoUserControl = null;
            try
            {
                if (this.isRunning)
                {
                    this.isRunning = false;
                    return;
                }

                taobaoUserControl = MainWindow.ProgramMainWindow.QueryUserControlInstance<AttachUI.TaobaoUserControl>();
                taobaoUserControl.OrderDownload += TaobaoUserControl_OrderDownload;
                taobaoUserControl.OrderPreviewDownload += TaobaoUserControl_OrderPreviewDownload;

                this.btnRefresh.Content = "停止";
                this.isRunning = true;
                this.orders.Clear();
                this.orderDownloads = new List<OrderDownload>();
                taobaoUserControl.DownloadOrders();
                if (this.orderDownloads == null || this.orderDownloads.Count < 1)
                {
                    MessageBox.Show("没有找到待发货的订单");
                    return;
                }
                var os = ServiceContainer.GetService<OrderService>();
                var orders = orderDownloads.Where(obj => obj.Order != null).Select(obj => obj.Order).Where(obj => string.IsNullOrWhiteSpace(obj.PopOrderId) == false && os.IsDBMinTime(obj.PopDeliveryTime)).Select(obj => new OrderViewModel(obj)).OrderBy(obj => obj.Source.PopPayTime).ToArray();
                //分析
                foreach (var order in orders)
                {
                    if (order.Source.State == OrderState.SHIPPED)
                    {
                        order.IsChecked = true;
                    }
                    this.orders.Add(order);
                }
                this.dgvOrders.ItemsSource = this.orders;
                this.tbTotal.Text = "当前共 : " + orders.Length + " 条记录";

                var error = orderDownloads.Where(obj => obj.Error != null).Select(obj => obj.Error).ToArray();
                if (error.Length > 0)
                {
                    string msg = string.Format("下载失败订单列表：\r\n{0}", string.Join(Environment.NewLine, error.Select(obj => obj.PopOrderId + ":" + obj.Error)));
                    MessageBox.Show(msg, "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                this.isRunning = false;
                this.btnRefresh.Content = "刷新";
                if (taobaoUserControl != null)
                {
                    taobaoUserControl.OrderDownload -= TaobaoUserControl_OrderDownload;
                    taobaoUserControl.OrderPreviewDownload -= TaobaoUserControl_OrderPreviewDownload;
                }
            }
        }

        private void TaobaoUserControl_OrderPreviewDownload(object sender, AttachUI.AttachUIOrderPreviewDownloadEventArgs e)
        {
            this.tbMsg.Text = string.Format("正在检测是否需要下载：{0}/{1} {2} ", e.Current, e.Total, e.PopOrderId);
            WPFHelper.DoEvents();

            if (this.isRunning == false)
            {
                e.Stop = true;
                return;
            }

            var odInDb = ServiceContainer.GetService<OrderService>().GetByPopOrderId(e.PopOrderId);
            if (odInDb.Total < 1)
            {
                return;
            }
            e.Skip = true;
            if (odInDb.First.PopFlag != e.PopFlag)
            {
                odInDb.First.PopSellerComment = (sender as AttachUI.IAttachUI).GetSellerComment(e.PopOrderId);
                odInDb.First.PopFlag = e.PopFlag;
                ServiceContainer.GetService<OrderService>().Update(odInDb.First);
            }

            OrderDownload od = new OrderDownload { Order = odInDb.First };
            this.orderDownloads.Add(od);
            var state = e.State;
            if (od.Order.State == state || od.Order.State == OrderState.CLOSED || od.Order.State == OrderState.CANCLED)
            {
                return;
            }
            if (state == OrderState.RETURNING || state == OrderState.CLOSED || state == OrderState.CANCLED)
            {
                od.Order.State = state;
                ServiceContainer.GetService<OrderService>().Update(od.Order);
            }
        }

        private void TaobaoUserControl_OrderDownload(object sender, AttachUI.AttachUiOrderDownloadEventArgs e)
        {
            if (e.OrderDownload.Order != null)
            {
                List<OrderDownload> downloads = new List<OrderDownload>();
                downloads.Add(e.OrderDownload);
                var ret = ServiceContainer.GetService<OrderService>().SaveOrUpdateOrdersByPopOrderId(ServiceContainer.GetService<ShopService>().GetById(e.OrderDownload.Order.ShopId), downloads);
                this.orderDownloads.AddRange(ret.Datas);
            }
            else
            {
                this.orderDownloads.Add(e.OrderDownload);
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

                if (so.Any(obj => obj.Source.State == OrderState.RETURNING))
                {
                    if (MessageBox.Show("所选订单中含有退款中订单是否确认发货？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                var dcs = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas;
                foreach (var o in so)
                {
                    WPFHelper.DoEvents();
                    try
                    {
                        if (string.IsNullOrEmpty(o.DeliveryNumber))
                        {
                            throw new Exception("快递单号为空");
                        }
                        var dc = dcs.FirstOrDefault(obj => obj.Name == o.DeliveryCompany).PopMapTaobao;
                        MainWindow.ProgramMainWindow.QueryUserControlInstance<AttachUI.TaobaoUserControl>().MarkPopDelivery(o.Source.PopOrderId, dc, o.DeliveryNumber);
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

        private void miSelectPre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.GetMIOrder(sender);
                MenuItem mi = sender as MenuItem;
                var orders = this.orders;
                int index = orders.IndexOf(item);
                for (int i = 0; i < orders.Count; i++)
                {
                    orders[i].IsChecked = i <= index ? true : false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void miSelectForward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.GetMIOrder(sender);
                MenuItem mi = sender as MenuItem;
                var orders = this.orders;
                int index = orders.IndexOf(item);

                for (int i = 0; i < orders.Count; i++)
                {
                    orders[i].IsChecked = i >= index ? true : false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void DgvOrders_Sorting(object sender, DataGridSortingEventArgs e)
        {
            try
            {
                string sortPath = e.Column.SortMemberPath;
                if (this.orders.Count < 1)
                {
                    return;
                }
                var sortType = e.Column.SortDirection == null ? ListSortDirection.Ascending : (e.Column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending);
                List<OrderViewModel> newVms = null;

                EnumerableKeySelector selector = new EnumerableKeySelector(orders[0].GetType(), sortPath);
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = orders.OrderBy(obj => selector.GetData(obj)).ToList();
                }
                else
                {
                    newVms = orders.OrderByDescending(obj => selector.GetData(obj)).ToList();
                }
                this.orders.Clear();
                foreach (var v in newVms)
                {
                    this.orders.Add(v);
                }
                e.Column.SortDirection = sortType;
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}